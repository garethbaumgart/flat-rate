using FluentAssertions;
using FlatRate.Domain.Aggregates.Properties;

namespace FlatRate.Domain.Tests.Aggregates.Properties;

public class PropertyAccessTests
{
    #region CreateForUser - Valid Scenarios

    [Fact]
    public void CreateForUser_WithValidInputs_ReturnsActiveAccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var access = PropertyAccess.CreateForUser(propertyId, userId, PropertyRole.Editor);

        // Assert
        access.PropertyId.Should().Be(propertyId);
        access.UserId.Should().Be(userId);
        access.IsActive.Should().BeTrue();
        access.IsPending.Should().BeFalse();
        access.AcceptedAt.Should().NotBeNull();
        access.InvitedEmail.Should().BeNull();
        access.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(PropertyRole.Owner)]
    [InlineData(PropertyRole.Editor)]
    public void CreateForUser_SetsRoleCorrectly(PropertyRole role)
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var access = PropertyAccess.CreateForUser(propertyId, userId, role);

        // Assert
        access.Role.Should().Be(role);
    }

    [Fact]
    public void CreateForUser_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var access = PropertyAccess.CreateForUser(propertyId, userId, PropertyRole.Editor);

        // Assert
        access.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region CreateForUser - Invalid Scenarios

    [Fact]
    public void CreateForUser_WithEmptyPropertyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreateForUser(Guid.Empty, Guid.NewGuid(), PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property ID cannot be empty*");
    }

    [Fact]
    public void CreateForUser_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreateForUser(Guid.NewGuid(), Guid.Empty, PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    #endregion

    #region CreatePendingInvite - Valid Scenarios

    [Fact]
    public void CreatePendingInvite_WithValidInputs_ReturnsPendingAccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var email = "user@example.com";

        // Act
        var access = PropertyAccess.CreatePendingInvite(propertyId, email, PropertyRole.Editor);

        // Assert
        access.PropertyId.Should().Be(propertyId);
        access.UserId.Should().BeNull();
        access.IsPending.Should().BeTrue();
        access.IsActive.Should().BeFalse();
        access.AcceptedAt.Should().BeNull();
        access.InvitedEmail.Should().Be("user@example.com");
        access.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void CreatePendingInvite_NormalizesEmailToLowercase()
    {
        // Arrange & Act
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "USER@EXAMPLE.COM", PropertyRole.Editor);

        // Assert
        access.InvitedEmail.Should().Be("user@example.com");
    }

    [Fact]
    public void CreatePendingInvite_TrimsEmail()
    {
        // Arrange & Act
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "  user@example.com  ", PropertyRole.Editor);

        // Assert
        access.InvitedEmail.Should().Be("user@example.com");
    }

    #endregion

    #region CreatePendingInvite - Invalid Scenarios

    [Fact]
    public void CreatePendingInvite_WithEmptyPropertyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreatePendingInvite(Guid.Empty, "user@example.com", PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property ID cannot be empty*");
    }

    [Fact]
    public void CreatePendingInvite_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreatePendingInvite(Guid.NewGuid(), null!, PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void CreatePendingInvite_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "", PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void CreatePendingInvite_WithWhitespaceEmail_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "   ", PropertyRole.Editor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    #endregion

    #region AcceptInvite

    [Fact]
    public void AcceptInvite_OnPendingAccess_BecomesActive()
    {
        // Arrange
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "user@example.com", PropertyRole.Editor);
        var userId = Guid.NewGuid();

        // Act
        access.AcceptInvite(userId);

        // Assert
        access.UserId.Should().Be(userId);
        access.IsActive.Should().BeTrue();
        access.IsPending.Should().BeFalse();
        access.InvitedEmail.Should().BeNull();
        access.AcceptedAt.Should().NotBeNull();
        access.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AcceptInvite_OnAlreadyActiveAccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var access = PropertyAccess.CreateForUser(Guid.NewGuid(), Guid.NewGuid(), PropertyRole.Editor);

        // Act
        var act = () => access.AcceptInvite(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("This access is already active.");
    }

    [Fact]
    public void AcceptInvite_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "user@example.com", PropertyRole.Editor);

        // Act
        var act = () => access.AcceptInvite(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    #endregion

    #region State Properties

    [Fact]
    public void IsPending_WhenUserIdNullAndEmailSet_ReturnsTrue()
    {
        // Arrange
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "user@example.com", PropertyRole.Editor);

        // Act & Assert
        access.IsPending.Should().BeTrue();
    }

    [Fact]
    public void IsPending_WhenUserIdSet_ReturnsFalse()
    {
        // Arrange
        var access = PropertyAccess.CreateForUser(Guid.NewGuid(), Guid.NewGuid(), PropertyRole.Editor);

        // Act & Assert
        access.IsPending.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenUserIdSet_ReturnsTrue()
    {
        // Arrange
        var access = PropertyAccess.CreateForUser(Guid.NewGuid(), Guid.NewGuid(), PropertyRole.Editor);

        // Act & Assert
        access.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenUserIdNull_ReturnsFalse()
    {
        // Arrange
        var access = PropertyAccess.CreatePendingInvite(Guid.NewGuid(), "user@example.com", PropertyRole.Editor);

        // Act & Assert
        access.IsActive.Should().BeFalse();
    }

    #endregion
}
