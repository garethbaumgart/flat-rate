using System.Security.Claims;
using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Users;

namespace FlatRate.Web.Services;

/// <summary>
/// Implementation of ICurrentUserService that extracts user info from HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private Guid? _cachedUserId;
    private bool _userIdLookedUp;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public Guid? UserId
    {
        get
        {
            if (_userIdLookedUp)
                return _cachedUserId;

            _userIdLookedUp = true;

            if (GoogleId is null)
                return null;

            // Look up user by Google ID
            using var scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var user = userRepository.GetByGoogleIdAsync(GoogleId).GetAwaiter().GetResult();
            _cachedUserId = user?.Id;

            return _cachedUserId;
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
