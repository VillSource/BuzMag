using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Domain;
using Villsource.ObjectValue;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Ref).IsUnique();
        builder.HasIndex(e => e.Code).IsUnique();

        builder.Property(e => e.Ref).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(e => e.UserId).HasMaxLength(36); 
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();

        builder.Property(e => e.Title).HasMaxLength(30).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MiddleName).HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.TitleEn).HasMaxLength(30).IsRequired();
        builder.Property(e => e.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MiddleNameEn).HasMaxLength(100);
        builder.Property(e => e.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();

        builder.Property(e => e.SnapshotManagerRef).HasMaxLength(VillsourceId.KeySizes);
        builder.Property(e => e.SnapshotOuRef).HasMaxLength(VillsourceId.KeySizes);
        builder.Property(e => e.SnapshotPositionRef).HasMaxLength(VillsourceId.KeySizes);
        builder.Property(e => e.SnapshotTier)
            .HasConversion(
                tier => tier != null ? tier.Key : null,
                value => ToPositionTier(value))
            .HasMaxLength(50);
        builder.ComplexProperty(e => e.Address).ConfigureAddress();

        builder.Property(e => e.Status)
            .HasConversion(
                status => status.Key == EmployeeStatus.None.Key ? null : status.Key,
                value => toEmployeeStatus(value));
        
        builder.HasMany(e => e.Employments)
            .WithOne()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction); 

        builder.HasMany(e => e.PositionAssignments)
            .WithOne()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(e => e.Employments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.PositionAssignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static PositionTier? ToPositionTier(string? key) => PositionTier.TryGet(key, out PositionTier? tier) ? tier : null;
    private static EmployeeStatus toEmployeeStatus(string? key) => EmployeeStatus.TryGet(key, out EmployeeStatus? status) ? status : EmployeeStatus.None;
}