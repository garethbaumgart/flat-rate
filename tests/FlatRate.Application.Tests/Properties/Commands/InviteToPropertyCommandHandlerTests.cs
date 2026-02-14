using FluentAssertions;
using FlatRate.Application.Common;
using FlatRate.Application.Properties.Commands.InviteToProperty;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using FlatRate.Domain.Repositories;
using NSubstitute;

namespace FlatRate.Application.Tests.Properties.Commands;

public class InviteToPropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly InviteToPropertyCommandHandler _handler;

    public InviteToPropertyCommandHandlerTests()
    {
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _propertyAccessRepository = Substitute.For<IPropertyAccessRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        _handler = new InviteToPropertyCommandHandler(
            _propertyRepository,
            _propertyAccessRepository,
            _userRepository,
            _unitOfWork,
            _currentUserService);
    }

    #region Validation Rejection Paths

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsError()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new InviteToPropertyCommand(Guid.NewGuid(), "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WithNullEmail_ReturnsError()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        var command = new InviteToPropertyCommand(Guid.NewGuid(), null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Please enter a valid email address.");
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsError()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        var command = new InviteToPropertyCommand(Guid.NewGuid(), "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Please enter a valid email address.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    [InlineData("no spaces@test.com")]
    public async Task Handle_WithInvalidEmailFormat_ReturnsError(string email)
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        var command = new InviteToPropertyCommand(Guid.NewGuid(), email);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Please enter a valid email address.");
    }

    [Fact]
    public async Task Handle_WhenPropertyNotFound_ReturnsError()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var command = new InviteToPropertyCommand(propertyId, "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Property not found.");
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ReturnsError()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Editor);

        var command = new InviteToPropertyCommand(propertyId, "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Only the property owner can invite collaborators.");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasAccess_ReturnsError()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");
        var existingUser = User.Create("google-123", "user@example.com", "Existing User");
        var existingAccess = PropertyAccess.CreateForUser(propertyId, existingUserId, PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _propertyAccessRepository.GetByPropertyAndUserAsync(propertyId, existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingAccess);

        var command = new InviteToPropertyCommand(propertyId, "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User already has access to this property.");
    }

    [Fact]
    public async Task Handle_WhenPendingInviteExists_ReturnsError()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");
        var pendingAccess = PropertyAccess.CreatePendingInvite(propertyId, "user@example.com", PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _propertyAccessRepository.GetPendingByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess> { pendingAccess });

        var command = new InviteToPropertyCommand(propertyId, "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("An invite has already been sent to this email.");
    }

    #endregion

    #region Success Paths

    [Fact]
    public async Task Handle_WhenExistingUser_CreatesActiveAccess()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");
        var existingUser = User.Create("google-123", "user@example.com", "Existing User");

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _propertyAccessRepository.GetByPropertyAndUserAsync(propertyId, existingUser.Id, Arg.Any<CancellationToken>())
            .Returns((PropertyAccess?)null);

        var command = new InviteToPropertyCommand(propertyId, "user@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _propertyAccessRepository.Received(1).AddAsync(
            Arg.Is<PropertyAccess>(a => a.UserId == existingUser.Id && a.IsActive),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_CreatesPendingInvite()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _userRepository.GetByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _propertyAccessRepository.GetPendingByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess>());

        var command = new InviteToPropertyCommand(propertyId, "newuser@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _propertyAccessRepository.Received(1).AddAsync(
            Arg.Is<PropertyAccess>(a => a.IsPending && a.InvitedEmail == "newuser@example.com"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DefaultsRoleToEditor()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _userRepository.GetByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _propertyAccessRepository.GetPendingByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns(new List<PropertyAccess>());

        // Command without explicit role — defaults to Editor
        var command = new InviteToPropertyCommand(propertyId, "newuser@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _propertyAccessRepository.Received(1).AddAsync(
            Arg.Is<PropertyAccess>(a => a.Role == PropertyRole.Editor),
            Arg.Any<CancellationToken>());
    }

    #endregion
}
