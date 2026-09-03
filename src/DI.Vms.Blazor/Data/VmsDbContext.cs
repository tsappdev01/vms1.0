using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Blazor.Data;

public class VmsDbContext(DbContextOptions<VmsDbContext> options) : DbContext(options)
{
    public DbSet<DiEntity> DiEntities => Set<DiEntity>();
    public DbSet<VisitorEntry> VisitorEntries => Set<VisitorEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("vms");

        b.Entity<DiEntity>(e =>
        {
            e.ToTable("Entity");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();

            // The DI group companies from BRD section 6, seeded so the entity dropdown is
            // populated on a fresh database without a separate script.
            e.HasData(
                new DiEntity { Id = 1, Name = "Dubai Investments PJSC" },
                new DiEntity { Id = 2, Name = "Dubai Investments Park" },
                new DiEntity { Id = 3, Name = "National General Insurance" },
                new DiEntity { Id = 4, Name = "Masharie" },
                new DiEntity { Id = 5, Name = "Glass Entities" },
                new DiEntity { Id = 6, Name = "Other DI Subsidiaries" },
                new DiEntity { Id = 7, Name = "Other DI Sister Companies" });
        });

        b.Entity<VisitorEntry>(e =>
        {
            e.ToTable("VisitorEntry");
            e.Property(x => x.IdNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.CardNumber).HasMaxLength(30);
            e.Property(x => x.FullNameEnglish).HasMaxLength(300).IsRequired();
            e.Property(x => x.FullNameRaw).HasMaxLength(300);
            e.Property(x => x.FullNameArabic).HasMaxLength(300);
            e.Property(x => x.PersonToVisit).HasMaxLength(200).IsRequired();
            e.Property(x => x.CaptureMethod).HasMaxLength(30).IsRequired();
            e.Property(x => x.AddressEmail).HasMaxLength(256);

            foreach (var name in new[]
            {
                nameof(VisitorEntry.IdType), nameof(VisitorEntry.IssueDate),
                nameof(VisitorEntry.ExpiryDate), nameof(VisitorEntry.TitleEnglish),
                nameof(VisitorEntry.Gender), nameof(VisitorEntry.DateOfBirth),
                nameof(VisitorEntry.NationalityEnglish), nameof(VisitorEntry.NationalityCode),
                nameof(VisitorEntry.PlaceOfBirthEnglish), nameof(VisitorEntry.AddressEmirate),
                nameof(VisitorEntry.AddressCity), nameof(VisitorEntry.AddressArea),
                nameof(VisitorEntry.AddressStreet), nameof(VisitorEntry.AddressBuilding),
                nameof(VisitorEntry.AddressPoBox), nameof(VisitorEntry.AddressPhone),
                nameof(VisitorEntry.AddressMobile),
            })
            {
                e.Property(name).HasMaxLength(150);
            }

            e.HasOne(x => x.DiEntity).WithMany().HasForeignKey(x => x.DiEntityId);

            // The report groups by entity and orders by time, so both are indexed.
            e.HasIndex(x => new { x.DiEntityId, x.RecordedAtUtc });
            e.HasIndex(x => x.IdNumber);
        });
    }
}
