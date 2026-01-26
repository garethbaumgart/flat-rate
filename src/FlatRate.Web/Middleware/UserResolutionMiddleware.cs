using System.Security.Claims;
using FlatRate.Domain.Aggregates.Users;

namespace FlatRate.Web.Middleware;

/// <summary>
/// Middleware that resolves the authenticated user's Google ID to an internal user ID.
/// This adds the internal_user_id claim to the user's identity for use by CurrentUserService.
/// </summary>
public sealed class UserResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public UserResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(googleId))
            {
                var user = await userRepository.GetByGoogleIdAsync(googleId);

                if (user is not null)
                {
                    // Add the internal user ID as a claim
                    var identity = context.User.Identity as ClaimsIdentity;
                    identity?.AddClaim(new Claim("internal_user_id", user.Id.ToString()));
                }
            }
        }

        await _next(context);
    }
}

public static class UserResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseUserResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserResolutionMiddleware>();
    }
}
