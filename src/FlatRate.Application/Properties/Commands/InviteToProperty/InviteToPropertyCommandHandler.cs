using System.Text.RegularExpressions;
using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.InviteToProperty;

/// <summary>
/// Handler for InviteToPropertyCommand.
/// </summary>
public sealed class InviteToPropertyCommandHandler : IRequestHandler<InviteToPropertyCommand, InviteToPropertyResult>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public InviteToPropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IPropertyAccessRepository propertyAccessRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _propertyAccessRepository = propertyAccessRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<InviteToPropertyResult> Handle(InviteToPropertyCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return new InviteToPropertyResult(false, "User must be authenticated.");
        }

        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex.IsMatch(request.Email.Trim()))
        {
            return new InviteToPropertyResult(false, "Please enter a valid email address.");
        }

        // Verify property exists
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, cancellationToken);
        if (property is null)
        {
            return new InviteToPropertyResult(false, "Property not found.");
        }

        // Only owner can invite others
        var currentUserRole = await _propertyRepository.GetUserRoleAsync(request.PropertyId, _currentUserService.UserId.Value, cancellationToken);
        if (currentUserRole != PropertyRole.Owner)
        {
            return new InviteToPropertyResult(false, "Only the property owner can invite collaborators.");
        }

        // Normalize email
        var email = request.Email.Trim().ToLowerInvariant();

        // Check if user with this email already has access
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            var existingAccess = await _propertyAccessRepository.GetByPropertyAndUserAsync(request.PropertyId, existingUser.Id, cancellationToken);
            if (existingAccess is not null)
            {
                return new InviteToPropertyResult(false, "User already has access to this property.");
            }

            // Grant access immediately since user exists
            var access = PropertyAccess.CreateForUser(request.PropertyId, existingUser.Id, request.Role);
            await _propertyAccessRepository.AddAsync(access, cancellationToken);
        }
        else
        {
            // Check if there's already a pending invite for this email
            var pendingInvites = await _propertyAccessRepository.GetPendingByEmailAsync(email, cancellationToken);
            if (pendingInvites.Any(p => p.PropertyId == request.PropertyId))
            {
                return new InviteToPropertyResult(false, "An invite has already been sent to this email.");
            }

            // Create pending invite
            var access = PropertyAccess.CreatePendingInvite(request.PropertyId, email, request.Role);
            await _propertyAccessRepository.AddAsync(access, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InviteToPropertyResult(true);
    }
}
