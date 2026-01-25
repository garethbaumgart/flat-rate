using FlatRate.Application;
using FlatRate.Application.Common;
using FlatRate.Application.Users.Commands.EnsureUserExists;
using FlatRate.Infrastructure;
using FlatRate.Infrastructure.Persistence;
using FlatRate.Web.Auth;
using FlatRate.Web.Endpoints;
using FlatRate.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

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
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/auth/google-callback";
    });
}

// Store whether Google is configured for use in endpoints
builder.Services.AddSingleton(new AuthConfiguration { GoogleConfigured = googleConfigured });

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply database schema on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FlatRateDbContext>();
    // In development/testing, use EnsureCreated for simplicity
    // In production, this should use migrations
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }
}

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint (no auth required)
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

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
    var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

    if (string.IsNullOrEmpty(googleId))
    {
        return Results.Unauthorized();
    }

    // Ensure user exists in database (creates if new, updates last login if existing)
    var userId = await mediator.Send(new EnsureUserExistsCommand(googleId, email, name));

    return Results.Ok(new
    {
        id = userId,
        googleId = googleId,
        name = name,
        email = email
    });
}).RequireAuthorization();

app.Run();
