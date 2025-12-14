using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using FinanceService.Application.Commands;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;

namespace FinanceService.Tests.Commands;

public class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<ILogger<CreateInvoiceCommandHandler>> _loggerMock;
    private readonly CreateInvoiceCommandHandler _handler;

    public CreateInvoiceCommandHandlerTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _loggerMock = new Mock<ILogger<CreateInvoiceCommandHandler>>();
        _handler = new CreateInvoiceCommandHandler(
            _invoiceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewBooking_ShouldCreateInvoice()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateInvoiceCommand(
            bookingId,
            userId,
            "test@example.com",
            "Test User",
            "Test Booking",
            150.00m,
            DateTime.UtcNow);

        _invoiceRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().NotBeEmpty();
        result.Amount.Should().Be(150.00m);
        result.InvoiceNumber.Should().StartWith("INV-");

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateBooking_ShouldReturnExistingInvoice_ForIdempotency()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingInvoice = Invoice.Create(
            bookingId,
            userId,
            "test@example.com",
            "Test User",
            "Test Booking",
            150.00m,
            DateTime.UtcNow);

        var command = new CreateInvoiceCommand(
            bookingId,
            userId,
            "test@example.com",
            "Test User",
            "Test Booking",
            150.00m,
            DateTime.UtcNow);

        _invoiceRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(existingInvoice.Id);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(999.99)]
    [InlineData(50.50)]
    public async Task Handle_DifferentAmounts_ShouldSucceed(decimal amount)
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateInvoiceCommand(
            bookingId,
            userId,
            "test@example.com",
            "Test User",
            "Test Booking",
            amount,
            DateTime.UtcNow);

        _invoiceRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result!.Amount.Should().Be(amount);
    }

    [Fact]
    public async Task Handle_IdempotencyCheck_ShouldBeCalledFirst()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateInvoiceCommand(
            bookingId,
            userId,
            "test@example.com",
            "Test User",
            "Test Booking",
            100m,
            DateTime.UtcNow);

        var callOrder = new List<string>();

        _invoiceRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("IdempotencyCheck"))
            .ReturnsAsync((Invoice?)null);

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Add"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("IdempotencyCheck", "Add");
    }
}
