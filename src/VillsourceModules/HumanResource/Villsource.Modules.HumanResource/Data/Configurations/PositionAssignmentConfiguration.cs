using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Data.Configurations;

public class PositionAssignmentConfiguration : IEntityTypeConfiguration<PositionAssignment>
{
    public void Configure(EntityTypeBuilder<PositionAssignment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PositionAssignments");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Ref).IsUnique();
        
        builder.Property(p => p.Ref).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(p => p.EmployeeId).IsRequired();
        builder.Property(p => p.OrganizationUnitRef).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(p => p.PositionRef).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(p => p.ManagerId);
        
        builder.HasOne(p=>p.Manager)
            .WithMany()
            .HasForeignKey(p=>p.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- SmartEnum Value Conversions ---
        builder.Property(p => p.PositionTier)
            .HasConversion(
                tier => tier.Key,
                key => ToPositionTier(key))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasConversion(
                type => type.Key,
                key => ToPositionAssignmentType(key))
            .HasMaxLength(50)
            .IsRequired();
    }

    private static PositionTier ToPositionTier(string? key) 
        => PositionTier.TryGet(key, out PositionTier? tier) ? tier : PositionTier.None;

    private static PositionAssignmentType ToPositionAssignmentType(string? key) 
        => PositionAssignmentType.TryGet(key, out PositionAssignmentType? type) ? type : PositionAssignmentType.None;
}