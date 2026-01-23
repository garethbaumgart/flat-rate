using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
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

        var userId = headerValue.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, $"Mock User ({userId})"),
            new Claim(ClaimTypes.Email, $"{userId}@mock.local")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
