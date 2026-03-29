using Microsoft.EntityFrameworkCore;
using MvDomain.Entities;

namespace MvInfrastructure.Data;

public class TicketingDbContext(DbContextOptions<TicketingDbContext> options) : DbContext(options) {
  public DbSet<Event> Events => Set<Event>();
  public DbSet<TicketOrder> TicketOrders => Set<TicketOrder>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Event>(builder => {
      builder.ToTable("Events");
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
      builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
      builder.Property(x => x.Venue).HasMaxLength(200).IsRequired();
      builder.Property(x => x.TicketPrice).HasColumnType("decimal(18,2)");
      builder.Property(x => x.PosterUrl).HasMaxLength(1000);
    });

    modelBuilder.Entity<TicketOrder>(builder => {
      builder.ToTable("TicketOrders");
      builder.HasKey(x => x.Id);
      builder.Property(x => x.EventName).HasMaxLength(200).IsRequired();
      builder.Property(x => x.CustomerEmail).HasMaxLength(250).IsRequired();
      builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
      builder.Property(x => x.PaymentProvider).HasConversion<string>().HasMaxLength(50);
      builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
      builder.Property(x => x.PaymentUrl).HasMaxLength(2000);
      builder.Property(x => x.GatewayReferenceId).HasMaxLength(200);
      builder.Property(x => x.GatewayTransactionId).HasMaxLength(200);
      builder.Property(x => x.TicketCode).HasMaxLength(100);
      builder.Property(x => x.FailureReason).HasMaxLength(2000);
      builder.HasIndex(x => x.GatewayReferenceId);
      builder.HasIndex(x => new { x.EventId, x.CustomerEmail, x.CreatedAt });
    });
  }
}
