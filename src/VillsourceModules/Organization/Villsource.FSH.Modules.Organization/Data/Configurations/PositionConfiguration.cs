using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Villsource.Tool.UniqueKey;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;
using PositionRules = Villsource.FSH.Modules.Organization.Domain.Positions.PositionRules;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PositionsV2");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Unique & Hierarchy Lookup Indexes
        builder.HasIndex(p => p.ReferenceId).IsUnique();
        builder.HasIndex(p => new { p.LegalEntityId, p.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(p => new { p.LegalEntityId, p.OrgUnitId, p.IsActive })
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(p => new { p.LegalEntityId, p.ReportsToPositionId })
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(p => p.JobGradeId);
        builder.HasIndex(p => p.JobId);
        builder.HasIndex(p => p.CostCenterId);

        // Column Constraints (Single Source of Truth from PositionRules)
        builder.Property(p => p.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(PositionRules.MaxCodeLength).IsRequired();
        builder.Property(p => p.Title).HasMaxLength(PositionRules.MaxTitleLength).IsRequired();
        builder.Property(p => p.InternationalTitle).HasMaxLength(PositionRules.MaxTitleLength).IsRequired();

        // Relationships
        builder.HasOne(p => p.LegalEntity)
            .WithMany()
            .HasForeignKey(p => p.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Job)
            .WithMany()
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.JobGrade)
            .WithMany()
            .HasForeignKey(p => p.JobGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CostCenter)
            .WithMany()
            .HasForeignKey(p => p.CostCenterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.ReportsToPosition)
            .WithMany(p => p.DirectReports)
            .HasForeignKey(p => p.ReportsToPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.DirectReports)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(p => p.DottedLinePosition)
            .WithMany()
            .HasForeignKey(p => p.DottedLinePositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}