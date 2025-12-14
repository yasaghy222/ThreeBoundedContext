using Microsoft.EntityFrameworkCore;
using FinanceService.Domain.Entities;

namespace FinanceService.Infrastructure.Persistence;

public class FinanceDbContext : DbContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(builder =>
        {
            builder.ToTable("Invoices");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Id)
                .ValueGeneratedNever();

            builder.Property(i => i.BookingId)
                .IsRequired();

            builder.HasIndex(i => i.BookingId)
                .IsUnique();

            builder.Property(i => i.UserId)
                .IsRequired();

            builder.HasIndex(i => i.UserId);

            builder.Property(i => i.UserEmail)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(i => i.UserFullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.BookingDescription)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(i => i.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(i => i.BookingDate)
                .IsRequired();

            builder.Property(i => i.InvoiceNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            builder.Property(i => i.Status)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.Property(i => i.PaidAt);

            builder.Property(i => i.DueDate)
                .IsRequired();
        });
    }
}
