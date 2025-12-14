using Microsoft.EntityFrameworkCore;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Outbox;

namespace BookingService.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
	public DbSet<Booking> Bookings => Set<Booking>();
	public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

	public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Booking>(builder =>
		{
			builder.ToTable("Bookings");

			builder.HasKey(b => b.Id);

			builder.Property(b => b.Id)
		  .ValueGeneratedNever();

			builder.Property(b => b.UserId)
		  .IsRequired();

			builder.HasIndex(b => b.UserId);

			builder.Property(b => b.UserEmail)
		  .HasMaxLength(256)
		  .IsRequired();

			builder.Property(b => b.UserFullName)
		  .HasMaxLength(200)
		  .IsRequired();

			builder.Property(b => b.Description)
		  .HasMaxLength(500)
		  .IsRequired();

			builder.Property(b => b.Amount)
		  .HasPrecision(18, 2)
		  .IsRequired();

			builder.Property(b => b.BookingDate)
		  .IsRequired();

			builder.Property(b => b.Status)
		  .IsRequired();

			builder.Property(b => b.CreatedAt)
		  .IsRequired();

			builder.Property(b => b.UpdatedAt);

			// Ignore domain events
			builder.Ignore(b => b.DomainEvents);
		});

		modelBuilder.Entity<OutboxMessage>(builder =>
		{
			builder.ToTable("OutboxMessages");

			builder.HasKey(o => o.Id);

			builder.Property(o => o.Type)
		  .HasMaxLength(500)
		  .IsRequired();

			builder.Property(o => o.Content)
		  .IsRequired();

			builder.Property(o => o.OccurredAt)
		  .IsRequired();

			builder.Property(o => o.ProcessedAt);

			builder.Property(o => o.Error)
		  .HasMaxLength(2000);

			builder.Property(o => o.RetryCount)
		  .HasDefaultValue(0);

			builder.HasIndex(o => o.ProcessedAt);
		});
	}
}
