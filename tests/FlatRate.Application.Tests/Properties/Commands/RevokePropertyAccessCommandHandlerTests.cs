using FluentAssertions;
using FlatRate.Application.Common;
using FlatRate.Application.Properties.Commands.RevokePropertyAccess;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using NSubstitute;

namespace FlatRate.Application.Tests.Properties.Commands;

public class RevokePropertyAccessCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly RevokePropertyAccessCommandHandler _handler;

    public RevokePropertyAccessCommandHandlerTests()
    {
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _propertyAccessRepository = Substitute.For<IPropertyAccessRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        _handler = new RevokePropertyAccessCommandHandler(
            _propertyRepository,
            _propertyAccessRepository,
            _unitOfWork,
            _currentUserService);
    }

    #region Validation Rejection Paths

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsError()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new RevokePropertyAccessCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User must be authenticated.");
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

        var command = new RevokePropertyAccessCommand(propertyId, Guid.NewGuid());

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

        var command = new RevokePropertyAccessCommand(propertyId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Only the property owner can revoke access.");
    }

    [Fact]
    public async Task Handle_WhenRevokingSelf_ReturnsError()
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

        var command = new RevokePropertyAccessCommand(propertyId, currentUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("You cannot revoke your own access.");
    }

    [Fact]
    public async Task Handle_WhenAccessNotFound_ReturnsError()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _propertyAccessRepository.GetByPropertyAndUserAsync(propertyId, targetUserId, Arg.Any<CancellationToken>())
            .Returns((PropertyAccess?)null);

        var command = new RevokePropertyAccessCommand(propertyId, targetUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User does not have access to this property.");
    }

    #endregion

    #region Success Path

    [Fact]
    public async Task Handle_WhenValid_DeletesAccessAndSaves()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var property = Property.Create("Test Property", "123 Test St");
        var access = PropertyAccess.CreateForUser(propertyId, targetUserId, PropertyRole.Editor);

        _currentUserService.UserId.Returns(currentUserId);
        _propertyRepository.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _propertyRepository.GetUserRoleAsync(propertyId, currentUserId, Arg.Any<CancellationToken>())
            .Returns(PropertyRole.Owner);
        _propertyAccessRepository.GetByPropertyAndUserAsync(propertyId, targetUserId, Arg.Any<CancellationToken>())
            .Returns(access);

        var command = new RevokePropertyAccessCommand(propertyId, targetUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _propertyAccessRepository.Received(1).Delete(access);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
