using DI.Vms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Infrastructure.Persistence;

/// <summary>
/// Maps onto the hand-written schema in db/schema. The SQL is the source of truth:
/// this context is configured to match it, and does not generate it.
/// </summary>
public class VmsDbContext(DbContextOptions<VmsDbContext> options) : DbContext(options)
{
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<VisitorSignature> Signatures => Set<VisitorSignature>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<DiEntity> DiEntities => Set<DiEntity>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("vms");

        b.Entity<DiEntity>(e =>
        {
            e.ToTable("DiEntity");
            e.Property(x => x.EntityCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.EntityCode).IsUnique();
        });

        b.Entity<Building>(e =>
        {
            e.ToTable("Building");
            e.Property(x => x.BuildingName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200);
        });

        b.Entity<Floor>(e =>
        {
            e.ToTable("Floor");
            e.Property(x => x.FloorNo).HasMaxLength(10).IsRequired();
            e.Property(x => x.FloorName).HasMaxLength(100);
            e.HasOne(x => x.Building).WithMany(x => x.Floors).HasForeignKey(x => x.BuildingId);
        });

        b.Entity<Office>(e =>
        {
            e.ToTable("Office");
            e.Property(x => x.OfficeNo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Department).HasMaxLength(100);
            e.HasOne(x => x.Floor).WithMany(x => x.Offices).HasForeignKey(x => x.FloorId);
        });

        b.Entity<Employee>(e =>
        {
            e.ToTable("Employee");
            e.Property(x => x.EmployeeCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.Designation).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Mobile).HasMaxLength(30);
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasOne(x => x.DiEntity).WithMany(x => x.Employees).HasForeignKey(x => x.DiEntityId);
            e.HasOne(x => x.Floor).WithMany().HasForeignKey(x => x.FloorId);
            e.HasOne(x => x.Office).WithMany().HasForeignKey(x => x.OfficeId);
        });

        b.Entity<User>(e =>
        {
            e.ToTable("User");
            e.Property(x => x.Username).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasConversion<byte>();
            e.Property(x => x.ExternalObjectId).HasMaxLength(64);
            e.Property(x => x.SecurityLocation).HasMaxLength(100);
            e.HasIndex(x => x.Username).IsUnique();
        });

        b.Entity<Visitor>(e =>
        {
            e.ToTable("Visitor");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Company).HasMaxLength(200);
            e.Property(x => x.IdType).HasConversion<byte>();
            e.Property(x => x.IdNumberCipher).HasColumnType("varbinary(512)").IsRequired();
            e.Property(x => x.IdNumberHash).HasColumnType("binary(32)").IsRequired();
            e.Property(x => x.IdNumberMasked).HasMaxLength(30).IsRequired();
            e.Property(x => x.Nationality).HasMaxLength(100);
            e.Property(x => x.CaptureMethod).HasConversion<byte>();
            e.HasIndex(x => new { x.IdType, x.IdNumberHash }).IsUnique();

            // Owned, so the address lives in the Visitor row rather than a join.
            e.OwnsOne(x => x.Address, a =>
            {
                a.Property(p => p.Emirate).HasColumnName("AddressEmirate").HasMaxLength(100);
                a.Property(p => p.City).HasColumnName("AddressCity").HasMaxLength(100);
                a.Property(p => p.Area).HasColumnName("AddressArea").HasMaxLength(150);
                a.Property(p => p.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                a.Property(p => p.Building).HasColumnName("AddressBuilding").HasMaxLength(200);
                a.Property(p => p.Flat).HasColumnName("AddressFlat").HasMaxLength(50);
                a.Property(p => p.PoBox).HasColumnName("AddressPoBox").HasMaxLength(50);
                a.Property(p => p.Mobile).HasColumnName("AddressMobile").HasMaxLength(50);
                a.Property(p => p.Email).HasColumnName("AddressEmail").HasMaxLength(256);
            });
        });

        b.Entity<Visit>(e =>
        {
            e.ToTable("Visit");
            e.Property(x => x.VisitNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Purpose).HasMaxLength(400);
            e.Property(x => x.VisitType).HasConversion<byte>();
            e.Property(x => x.Status).HasConversion<byte>();
            e.Property(x => x.Floor).HasColumnName("Floor").HasMaxLength(10);
            e.Property(x => x.Office).HasMaxLength(20);
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.CheckedInOnDeviceId).HasMaxLength(100);
            e.Ignore(x => x.Duration);
            e.Ignore(x => x.IsInside);
            e.HasIndex(x => x.VisitNumber).IsUnique();
            e.HasOne(x => x.Visitor).WithMany(x => x.Visits).HasForeignKey(x => x.VisitorId);
            e.HasOne(x => x.DiEntity).WithMany().HasForeignKey(x => x.DiEntityId);
            e.HasOne(x => x.HostEmployee).WithMany().HasForeignKey(x => x.HostEmployeeId);
            e.HasOne(x => x.Signature).WithOne(x => x.Visit).HasForeignKey<VisitorSignature>(x => x.VisitId);
        });

        b.Entity<VisitorSignature>(e =>
        {
            e.ToTable("VisitorSignature");
            e.Property(x => x.DeviceId).HasMaxLength(100);
            e.HasIndex(x => x.VisitId).IsUnique();
        });

        b.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog");
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Action).HasMaxLength(60).IsRequired();
            e.Property(x => x.EntityName).HasMaxLength(60).IsRequired();
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.Property(x => x.DeviceId).HasMaxLength(100);
        });
    }
}
