using Microsoft.EntityFrameworkCore;

namespace CrisilPreOnboardingApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PreOnboardingEntity> PreOnboardings => Set<PreOnboardingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PreOnboardingEntity>(b =>
        {
            b.ToTable("PreOnboardings");
            b.HasKey(x => x.Id);

            b.HasIndex(x => new { x.ExternalCandidateId, x.CrisilOfferId }).IsUnique();

            b.Property(x => x.ExternalCandidateId).HasMaxLength(50).IsRequired();
            b.Property(x => x.CrisilOfferId).HasMaxLength(50).IsRequired();
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.PersonalEmail).HasMaxLength(150).IsRequired();
            b.Property(x => x.MobileNumber).HasMaxLength(15).IsRequired();
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);
            b.Property(x => x.RawRequestJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();

        });
    }
}
