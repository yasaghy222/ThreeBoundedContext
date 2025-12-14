using FluentAssertions;
using UserService.Domain.Entities;
using UserService.Domain.Events;

namespace UserService.Tests.Domain;

public class UserTests
{
	[Fact]
	public void Create_ValidInput_ShouldCreateUser()
	{
		// Arrange
		var email = "test@example.com";
		var fullName = "Test User";
		var passwordHash = "hashedpassword";

		// Act
		var user = User.Create(email, fullName, passwordHash);

		// Assert
		user.Should().NotBeNull();
		user.Id.Should().NotBeEmpty();
		user.Email.Value.Should().Be(email);
		user.FullName.Should().Be(fullName);
		user.PasswordHash.Should().Be(passwordHash);
		user.IsActive.Should().BeTrue();
		user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
		user.UpdatedAt.Should().BeNull();
	}

	[Fact]
	public void Create_ShouldRaiseDomainEvent()
	{
		// Arrange & Act
		var user = User.Create("test@example.com", "Test User", "hash");

		// Assert
		user.DomainEvents.Should().HaveCount(1);
		user.DomainEvents.First().Should().BeOfType<UserCreatedDomainEvent>();

		var domainEvent = (UserCreatedDomainEvent)user.DomainEvents.First();
		domainEvent.UserId.Should().Be(user.Id);
		domainEvent.Email.Should().Be("test@example.com");
		domainEvent.FullName.Should().Be("Test User");
	}

	[Fact]
	public void Deactivate_ShouldSetIsActiveFalse()
	{
		// Arrange
		var user = User.Create("test@example.com", "Test User", "hash");
		user.ClearDomainEvents();

		// Act
		user.Deactivate();

		// Assert
		user.IsActive.Should().BeFalse();
		user.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public void Activate_ShouldSetIsActiveTrue()
	{
		// Arrange
		var user = User.Create("test@example.com", "Test User", "hash");
		user.ClearDomainEvents();
		user.Deactivate();

		// Act
		user.Activate();

		// Assert
		user.IsActive.Should().BeTrue();
		user.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public void UpdateProfile_ShouldUpdateFullName()
	{
		// Arrange
		var user = User.Create("test@example.com", "Old Name", "hash");
		user.ClearDomainEvents();

		// Act
		user.UpdateProfile("New Name");

		// Assert
		user.FullName.Should().Be("New Name");
		user.UpdatedAt.Should().NotBeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData(null)]
	public void Create_EmptyEmail_ShouldThrowException(string? email)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => User.Create(email!, "Test User", "hash"));
	}

	[Fact]
	public void Create_InvalidEmailFormat_ShouldThrowException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => User.Create("invalid-email", "Test User", "hash"));
	}
}
