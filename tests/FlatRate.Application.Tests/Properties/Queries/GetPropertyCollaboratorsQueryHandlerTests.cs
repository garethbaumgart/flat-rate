using FluentAssertions;
using FlatRate.Application.Common;
using FlatRate.Application.Properties.Queries.GetPropertyCollaborators;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using NSubstitute;

namespace FlatRate.Application.Tests.Properties.Queries;

public class GetPropertyCollaboratorsQueryHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetPropertyCollaboratorsQueryHandler _handler;

    public GetPropertyCollaboratorsQueryHandlerTests()
    {
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _propertyAccessRepository = Substitute.For<IPropertyAccessRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        _handler = new GetPropertyCollaboratorsQueryHandler(
            _propertyRepository,
            _propertyAccessRepository,
            _userRepository,
            _currentUserService);
    }

    #region Authentication and Access Checks

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsEmptyList()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetPropertyCollaboratorsQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoAccess_ReturnsEmptyList()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.UserHasAccessAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetPropertyCollaboratorsQuery(propertyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task Handle_MapsActiveCollaborator_WithUserDetails()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var collaboratorUserId = Guid.NewGuid();
        var user = User.Create("google-collab", "collab@example.com", "Collaborator User");

        var access = PropertyAccess.CreateForUser(propertyId, collaboratorUserId, PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.UserHasAccessAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(true);
        _propertyAccessRepository.GetByPropertyIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess> { access });
        _userRepository.GetByIdAsync(collaboratorUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetPropertyCollaboratorsQuery(propertyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.UserId.Should().Be(collaboratorUserId);
        dto.Email.Should().Be("collab@example.com");
        dto.Name.Should().Be("Collaborator User");
        dto.Role.Should().Be(PropertyRole.Editor);
        dto.IsPending.Should().BeFalse();
        dto.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MapsPendingCollaborator_WithEmailOnly()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var access = PropertyAccess.CreatePendingInvite(propertyId, "pending@example.com", PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.UserHasAccessAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(true);
        _propertyAccessRepository.GetByPropertyIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess> { access });

        var query = new GetPropertyCollaboratorsQuery(propertyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.UserId.Should().BeNull();
        dto.Email.Should().Be("pending@example.com");
        dto.Name.Should().BeNull();
        dto.Role.Should().Be(PropertyRole.Editor);
        dto.IsPending.Should().BeTrue();
        dto.AcceptedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserRecordMissing_ReturnsCollaboratorWithNullDetails()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var missingUserId = Guid.NewGuid();

        var access = PropertyAccess.CreateForUser(propertyId, missingUserId, PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.UserHasAccessAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(true);
        _propertyAccessRepository.GetByPropertyIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess> { access });
        _userRepository.GetByIdAsync(missingUserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetPropertyCollaboratorsQuery(propertyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — collaborator is included with null email/name
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.UserId.Should().Be(missingUserId);
        dto.Email.Should().BeNull();
        dto.Name.Should().BeNull();
        dto.IsPending.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsMixOfActiveAndPending()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var activeUserId = Guid.NewGuid();
        var activeUser = User.Create("google-active", "active@example.com", "Active User");

        var activeAccess = PropertyAccess.CreateForUser(propertyId, activeUserId, PropertyRole.Owner);
        var pendingAccess = PropertyAccess.CreatePendingInvite(propertyId, "pending@example.com", PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.UserHasAccessAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(true);
        _propertyAccessRepository.GetByPropertyIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess> { activeAccess, pendingAccess });
        _userRepository.GetByIdAsync(activeUserId, Arg.Any<CancellationToken>())
            .Returns(activeUser);

        var query = new GetPropertyCollaboratorsQuery(propertyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => !c.IsPending && c.UserId == activeUserId);
        result.Should().Contain(c => c.IsPending && c.Email == "pending@example.com");
    }

    #endregion
}
