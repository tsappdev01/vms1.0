using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Blazor.Data;

public class VmsDbContext(DbContextOptions<VmsDbContext> options) : DbContext(options)
{
    public DbSet<DiEntity> DiEntities => Set<DiEntity>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<VisitorEntry> VisitorEntries => Set<VisitorEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("vms");

        b.Entity<DiEntity>(e =>
        {
            e.ToTable("Entity");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();

            /* A database default, so an INSERT written by hand can leave IsActive out.
               EF's usual caveat - that it then omits the column whenever the value is the
               CLR default - does not bite here, because the application only ever reads
               this table. */
            e.Property(x => x.IsActive).HasDefaultValue(true);

            /* Not seeded from code at all - no HasData, no startup sync. The entity
               list is data owned by the database and maintained there by script, so it
               can be changed without a rebuild and a redeploy. */
        });

        b.Entity<Person>(e =>
        {
            e.ToTable("Person");
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.CompanyName).HasMaxLength(200);
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasOne(x => x.DiEntity).WithMany().HasForeignKey(x => x.DiEntityId);

            /* The picker matches anywhere in the name, so an index on DisplayName cannot
               serve a leading wildcard - it is here for the ordering, and because the
               table is small enough that a scan is cheap either way. */
            e.HasIndex(x => x.DisplayName);
            e.HasIndex(x => new { x.DiEntityId, x.IsActive });
        });

        b.Entity<VisitorEntry>(e =>
        {
            e.ToTable("VisitorEntry");
            e.Property(x => x.IdNumber).HasMaxLength(FieldLengths.IdNumber).IsRequired();
            e.Property(x => x.CardNumber).HasMaxLength(FieldLengths.CardNumber);
            e.Property(x => x.FullNameEnglish).HasMaxLength(FieldLengths.Name).IsRequired();
            e.Property(x => x.FullNameRaw).HasMaxLength(FieldLengths.Name);
            e.Property(x => x.FullNameArabic).HasMaxLength(FieldLengths.Name);
            e.Property(x => x.PersonToVisit).HasMaxLength(FieldLengths.PersonToVisit).IsRequired();
            e.Property(x => x.Purpose).HasMaxLength(FieldLengths.Purpose).IsRequired();
            e.Property(x => x.PurposeOther).HasMaxLength(FieldLengths.PurposeOther);
            e.Property(x => x.PersonToVisitTitle).HasMaxLength(FieldLengths.Title);
            e.Property(x => x.PersonToVisitEmail).HasMaxLength(FieldLengths.Email);
            e.Property(x => x.PersonToVisitCompany).HasMaxLength(FieldLengths.Company);

            /* NoAction, not Cascade: removing someone from the address list must never
               delete the visits that came to see them. */
            e.HasOne(x => x.PersonToVisitPerson)
             .WithMany()
             .HasForeignKey(x => x.PersonToVisitId)
             .OnDelete(DeleteBehavior.NoAction);
            e.Property(x => x.CaptureMethod).HasMaxLength(FieldLengths.CaptureMethod).IsRequired();
            e.Property(x => x.RecordedBy).HasMaxLength(FieldLengths.RecordedBy);
            e.Property(x => x.AddressEmail).HasMaxLength(FieldLengths.Email);

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
                e.Property(name).HasMaxLength(FieldLengths.CardField);
            }

            e.HasOne(x => x.DiEntity).WithMany().HasForeignKey(x => x.DiEntityId);

            // The report groups by entity and orders by time, so both are indexed.
            e.HasIndex(x => new { x.DiEntityId, x.RecordedAtUtc });
            e.HasIndex(x => x.IdNumber);
        });
    }
}
