using ErrorHandling.Core.Exceptions;
using FluentAssertions;
using Moq;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Domain.Common;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
	private readonly Mock<IUserRepository> _userRepositoryMock;
	private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
	private readonly RegisterUserCommandHandler _handler;

	public RegisterUserCommandHandlerTests()
	{
		_userRepositoryMock = new Mock<IUserRepository>();
		_domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
		_handler = new RegisterUserCommandHandler(
		    _userRepositoryMock.Object,
		    _domainEventDispatcherMock.Object);
	}

	[Fact]
	public async Task Handle_ValidCommand_ShouldCreateUser()
	{
		// Arrange
		var command = new RegisterUserCommand(
		    "john@example.com",
		    "John Doe",
		    "password123");

		_userRepositoryMock
		    .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
		    .ReturnsAsync(false);

		_userRepositoryMock
		    .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		_domainEventDispatcherMock
		    .Setup(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Email.Should().Be("john@example.com");
		result.FullName.Should().Be("John Doe");
		result.UserId.Should().NotBeEmpty();

		_userRepositoryMock.Verify(
		    x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
		    Times.Once);

		_domainEventDispatcherMock.Verify(
		    x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
		    Times.Once);
	}

	[Fact]
	public async Task Handle_ExistingEmail_ShouldThrowException()
	{
		// Arrange
		var command = new RegisterUserCommand(
		    "existing@example.com",
		    "John Doe",
		    "password123");

		_userRepositoryMock
		    .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
		    .ReturnsAsync(true);

		// Act & Assert
		await Assert.ThrowsAsync<ConflictException>(
		    () => _handler.Handle(command, CancellationToken.None));

		_userRepositoryMock.Verify(
		    x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
		    Times.Never);
	}

	[Theory]
	[InlineData("test@example.com", "John", "pass123")]
	[InlineData("user@domain.org", "Jane Smith", "securepassword")]
	public async Task Handle_DifferentValidInputs_ShouldSucceed(string email, string fullName, string password)
	{
		// Arrange
		var command = new RegisterUserCommand(email, fullName, password);

		_userRepositoryMock
		    .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
		    .ReturnsAsync(false);

		_userRepositoryMock
		    .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		_domainEventDispatcherMock
		    .Setup(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Email.Should().Be(email.ToLowerInvariant());
		result.FullName.Should().Be(fullName);
	}
}
