using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.ServiceLevel)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(c => c.Client)
                  .WithMany(cl => cl.Contracts)
                  .HasForeignKey(c => c.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CostUsd).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CostZar).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ExchangeRateUsed).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(sr => sr.Contract)
                  .WithMany(c => c.ServiceRequests)
                  .HasForeignKey(sr => sr.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
