using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
	public DbSet<User> Users => Set<User>();

	public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<User>(builder =>
		{
			builder.ToTable("Users");

			builder.HasKey(u => u.Id);

			builder.Property(u => u.Id)
		  .ValueGeneratedNever();

			builder.Property(u => u.Email)
		  .HasConversion(
		      e => e.Value,
		      v => Email.Create(v))
		  .HasMaxLength(256)
		  .IsRequired();

			builder.HasIndex(u => u.Email)
		  .IsUnique();

			builder.Property(u => u.FullName)
		  .HasMaxLength(200)
		  .IsRequired();

			builder.Property(u => u.PasswordHash)
		  .HasMaxLength(500)
		  .IsRequired();

			builder.Property(u => u.IsActive)
		  .IsRequired();

			builder.Property(u => u.CreatedAt)
		  .IsRequired();

			builder.Property(u => u.UpdatedAt);

			// Ignore domain events
			builder.Ignore(u => u.DomainEvents);
		});
	}
}
