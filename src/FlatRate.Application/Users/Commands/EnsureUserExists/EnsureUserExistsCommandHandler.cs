using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Users.Commands.EnsureUserExists;

/// <summary>
/// Handler for EnsureUserExistsCommand.
/// Creates a new user if they don't exist, updates last login if they do.
/// Also checks for any pending property invites by email and auto-grants them.
/// </summary>
public sealed class EnsureUserExistsCommandHandler : IRequestHandler<EnsureUserExistsCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnsureUserExistsCommandHandler(
        IUserRepository userRepository,
        IPropertyAccessRepository propertyAccessRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _propertyAccessRepository = propertyAccessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        // Normalize email for comparison
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if user already exists by Google ID
        var user = await _userRepository.GetByGoogleIdAsync(request.GoogleId, cancellationToken);

        if (user is not null)
        {
            // Update last login and profile info
            user.UpdateLastLogin();
            user.UpdateProfile(request.Name, request.Email, request.AvatarUrl);
            _userRepository.Update(user);
        }
        else
        {
            // Create new user
            user = User.Create(request.GoogleId, request.Email, request.Name, request.AvatarUrl);
            await _userRepository.AddAsync(user, cancellationToken);

            // Check for pending property invites by email and auto-grant them
            var pendingInvites = await _propertyAccessRepository.GetPendingByEmailAsync(normalizedEmail, cancellationToken);
            foreach (var invite in pendingInvites)
            {
                invite.AcceptInvite(user.Id);
                _propertyAccessRepository.Update(invite);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
