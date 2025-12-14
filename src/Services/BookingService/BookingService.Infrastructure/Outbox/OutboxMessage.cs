namespace BookingService.Infrastructure.Outbox;

public class OutboxMessage
{
	public Guid Id { get; set; }
	public string Type { get; set; } = null!;
	public string Content { get; set; } = null!;
	public DateTime OccurredAt { get; set; }
	public DateTime? ProcessedAt { get; set; }
	public string? Error { get; set; }
	public int RetryCount { get; set; }
}
