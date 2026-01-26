using System.Security.Claims;
using FlatRate.Application.Common;

namespace FlatRate.Web.Services;

/// <summary>
/// Implementation of ICurrentUserService that extracts user info from HTTP context.
/// The UserId is populated by UserResolutionMiddleware at the start of each request.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the internal user ID. This is populated by UserResolutionMiddleware.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("internal_user_id")?.Value;

            if (claim is not null && Guid.TryParse(claim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public string? GoogleId => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Email)?.Value;

    public string? Name => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
