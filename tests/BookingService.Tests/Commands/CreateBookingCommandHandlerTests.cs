using FluentAssertions;
using Moq;
using BookingService.Application.Commands;
using BookingService.Domain.Entities;
using BookingService.Domain.Repositories;
using BookingService.Domain.Services;

namespace BookingService.Tests.Commands;

public class CreateBookingCommandHandlerTests
{
	private readonly Mock<IBookingRepository> _bookingRepositoryMock;
	private readonly Mock<IUserValidationService> _userValidationServiceMock;
	private readonly CreateBookingCommandHandler _handler;

	public CreateBookingCommandHandlerTests()
	{
		_bookingRepositoryMock = new Mock<IBookingRepository>();
		_userValidationServiceMock = new Mock<IUserValidationService>();
		_handler = new CreateBookingCommandHandler(
		    _bookingRepositoryMock.Object,
		    _userValidationServiceMock.Object);
	}

	[Fact]
	public async Task Handle_ValidCommand_ShouldCreateBooking()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var command = new CreateBookingCommand(
		    userId,
		    "Test Service Description",
		    150.00m,
		    DateTime.UtcNow.AddDays(1));

		var userInfo = new UserInfo(userId, "test@example.com", "Test User", true);

		_userValidationServiceMock
		    .Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()))
		    .ReturnsAsync(userInfo);

		_bookingRepositoryMock
		    .Setup(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.UserId.Should().Be(userId);
		result.Description.Should().Be("Test Service Description");
		result.Amount.Should().Be(150.00m);
		result.Status.Should().Be("Pending");

		_userValidationServiceMock.Verify(
		    x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()),
		    Times.Once);

		_bookingRepositoryMock.Verify(
		    x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
		    Times.Once);
	}

	[Fact]
	public async Task Handle_UserNotFound_ShouldThrowException()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var command = new CreateBookingCommand(
		    userId,
		    "Test Service",
		    150.00m,
		    DateTime.UtcNow.AddDays(1));

		_userValidationServiceMock
		    .Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()))
		    .ReturnsAsync((UserInfo?)null);

		// Act & Assert
		await Assert.ThrowsAsync<InvalidOperationException>(
		    () => _handler.Handle(command, CancellationToken.None));

		_bookingRepositoryMock.Verify(
		    x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
		    Times.Never);
	}

	[Fact]
	public async Task Handle_InactiveUser_ShouldThrowException()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var command = new CreateBookingCommand(
		    userId,
		    "Test Service",
		    150.00m,
		    DateTime.UtcNow.AddDays(1));

		var inactiveUser = new UserInfo(userId, "test@example.com", "Test User", false);

		_userValidationServiceMock
		    .Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()))
		    .ReturnsAsync(inactiveUser);

		// Act & Assert
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
		    () => _handler.Handle(command, CancellationToken.None));

		exception.Message.Should().Contain("not active");

		_bookingRepositoryMock.Verify(
		    x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
		    Times.Never);
	}

	[Theory]
	[InlineData("Service A", 100.00)]
	[InlineData("Service B", 250.50)]
	[InlineData("Premium Service", 999.99)]
	public async Task Handle_DifferentServices_ShouldSucceed(string description, decimal amount)
	{
		// Arrange
		var userId = Guid.NewGuid();
		var command = new CreateBookingCommand(
		    userId,
		    description,
		    amount,
		    DateTime.UtcNow.AddDays(1));

		var userInfo = new UserInfo(userId, "user@example.com", "User Name", true);

		_userValidationServiceMock
		    .Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()))
		    .ReturnsAsync(userInfo);

		_bookingRepositoryMock
		    .Setup(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
		    .Returns(Task.CompletedTask);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Description.Should().Be(description);
		result.Amount.Should().Be(amount);
	}
}
