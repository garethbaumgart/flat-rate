using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace FlatRate.Web.Auth;

public class MockAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "MockAuth";
    public const string HeaderName = "X-Mock-User";
}

public class MockAuthenticationHandler(
    IOptionsMonitor<MockAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<MockAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(MockAuthenticationOptions.HeaderName, out var headerValue))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var raw = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Parse pipe-delimited format: "userId|Name|email" or just "userId"
        var parts = raw.Split('|');
        var userId = parts[0].Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var namePart = parts.Length > 1 ? parts[1].Trim() : "";
        var name = string.IsNullOrWhiteSpace(namePart) ? $"Mock User ({userId})" : namePart;

        var emailPart = parts.Length > 2 ? parts[2].Trim() : "";
        var email = string.IsNullOrWhiteSpace(emailPart) ? $"{userId}@mock.local" : emailPart;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // In development without Google configured, auto-sign-in as a mock user
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user-1"),
            new Claim(ClaimTypes.Name, "Developer"),
            new Claim(ClaimTypes.Email, "dev@flatrate.local")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await Context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        var returnUrl = properties.RedirectUri ?? "/";
        Context.Response.Redirect(returnUrl);
    }
}
