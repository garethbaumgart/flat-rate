using FlatRate.Application;
using FlatRate.Application.Common;
using FlatRate.Application.Users.Commands.EnsureUserExists;
using FlatRate.Infrastructure;
using FlatRate.Infrastructure.Persistence;
using FlatRate.Web.Auth;
using FlatRate.Web.Endpoints;
using FlatRate.Web.Middleware;
using FlatRate.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


// Add Application services (MediatR handlers)
builder.Services.AddApplication();

// Add Infrastructure services (EF Core, repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add PDF generation service
builder.Services.AddScoped<InvoicePdfService>();

// Add current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Serialize enums as strings in API responses (e.g. PropertyRole)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Authentication setup
var isDevelopment = builder.Environment.IsDevelopment();

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // In development, use MockAuth as the default challenge scheme (no Google redirect)
    options.DefaultChallengeScheme = isDevelopment
        ? MockAuthenticationOptions.Scheme
        : GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.Cookie.Name = "FlatRate.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    // Return 401 for API requests instead of redirecting to login page.
    // Without this, fetch() calls to /api/* get redirected to Google OAuth,
    // which fails due to CORS (browser can't follow cross-origin redirects from JS).
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    // Return 403 for API requests instead of redirecting to access denied page.
    // This avoids CORS/redirect issues when authenticated users lack permission
    // for a given /api resource.
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    // In development, forward auth to MockAuth scheme when mock header is present
    if (isDevelopment)
    {
        options.ForwardDefaultSelector = context =>
        {
            if (context.Request.Headers.ContainsKey(MockAuthenticationOptions.HeaderName))
            {
                return MockAuthenticationOptions.Scheme;
            }
            return null;
        };
    }
});

// Add mock authentication for development (must be added before Google)
if (isDevelopment)
{
    authBuilder.AddScheme<MockAuthenticationOptions, MockAuthenticationHandler>(
        MockAuthenticationOptions.Scheme, _ => { });
}

// Add Google authentication if configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var googleConfigured = !string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret);

if (googleConfigured)
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.CallbackPath = "/api/auth/google-callback";
        // When RequireAuthorization() triggers a Google challenge for API requests
        // (e.g. fetch('/api/auth/user')), return 401 instead of redirecting to Google.
        // Browser fetch() can't follow cross-origin redirects, causing CORS errors.
        // Exclude /api/auth/login since that's an intentional browser navigation to Google.
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api") &&
                !context.Request.Path.Equals("/api/auth/login"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
}

// Store whether Google is configured for use in endpoints
builder.Services.AddSingleton(new AuthConfiguration { GoogleConfigured = googleConfigured });

builder.Services.AddAuthorization();

var app = builder.Build();

// Cloud Run terminates TLS at the load balancer and forwards as HTTP.
// Force HTTPS scheme in production so OAuth generates correct redirect URIs.
if (!app.Environment.IsDevelopment())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
}

// Apply database migrations on startup
const int maxRetries = 5;
for (var i = 0; i < maxRetries; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlatRateDbContext>();
        dbContext.Database.Migrate();
        break;
    }
    catch (Exception ex) when (i < maxRetries - 1)
    {
        app.Logger.LogWarning(ex, "Database migration attempt {Attempt}/{Max} failed, retrying...", i + 1, maxRetries);
        Thread.Sleep(3000);
    }
}

// Serve static files from wwwroot/browser (Angular 21 output) when available
var webRootPath = app.Environment.WebRootPath;
var angularAppExists = !string.IsNullOrEmpty(webRootPath) && Directory.Exists(Path.Combine(webRootPath, "browser"));

if (angularAppExists)
{
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(webRootPath, "browser"))
    });
}

app.UseAuthentication();
app.UseUserResolution(); // Resolve Google ID to internal user ID
app.UseAuthorization();

// Health check endpoint (no auth required)
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

// API endpoints
app.MapPropertyEndpoints();
app.MapBillEndpoints();
app.MapPropertySharingEndpoints();

// Auth endpoints
app.MapGet("/api/auth/login", (HttpContext context, AuthConfiguration authConfig) =>
{
    var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault() ?? "/";

    // Use Google if configured, otherwise use MockAuth in development
    var scheme = authConfig.GoogleConfigured
        ? GoogleDefaults.AuthenticationScheme
        : MockAuthenticationOptions.Scheme;

    return Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, [scheme]);
});

app.MapGet("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapGet("/api/auth/user", async (HttpContext context, IMediator mediator) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var googleId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
    var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var pictureUrl = context.User.FindFirst("urn:google:picture")?.Value?.Trim();
    if (string.IsNullOrWhiteSpace(pictureUrl)) pictureUrl = null;

    if (string.IsNullOrEmpty(googleId))
    {
        return Results.Unauthorized();
    }

    // Email is required - OAuth providers that don't provide email are not supported
    if (string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(new { error = "Email address is required. Please use an account that provides email access." });
    }

    // Ensure user exists in database (creates if new, updates last login if existing)
    var userId = await mediator.Send(new EnsureUserExistsCommand(googleId, email, name, pictureUrl));

    return Results.Ok(new
    {
        id = userId,
        googleId = googleId,
        name = name,
        email = email,
        avatarUrl = pictureUrl
    });
}).RequireAuthorization();

// SPA fallback - serves index.html for client-side routing
if (angularAppExists)
{
    app.MapFallbackToFile("browser/index.html");
}

app.Run();
