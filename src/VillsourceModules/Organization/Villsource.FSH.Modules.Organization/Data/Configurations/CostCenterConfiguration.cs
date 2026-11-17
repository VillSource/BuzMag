using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("CostCenters");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        // Indexes
        builder.HasIndex(c => c.ReferenceId).IsUnique();
        builder.HasIndex(c => new { c.LegalEntityId, c.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(c => new { c.LegalEntityId, c.IsActive, c.IsPostingBlocked });
        builder.HasIndex(c => c.ParentCostCenterId);
        builder.HasIndex(c => c.FunctionType);
        builder.HasIndex(c => c.ResponsiblePositionId);

        // Properties
        builder.Property(c => c.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(250).IsRequired();
        builder.Property(c => c.InternationalName).HasMaxLength(250).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);

        // Thinktecture SmartEnum Conversion
        builder.Property(c => c.FunctionType)
            .HasConversion(
                v => v.Key,
                v => CostCenterFunctionType.Get(v))
            .HasMaxLength(50)
            .IsRequired();

        // Relationships (Soft-delete safe: NoAction / Restrict)
        builder.HasOne(c => c.LegalEntity)
            .WithMany()
            .HasForeignKey(c => c.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentCostCenter)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Children)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(c => c.DefaultTaxBranch)
            .WithMany()
            .HasForeignKey(c => c.DefaultTaxBranchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ResponsiblePosition)
            .WithMany()
            .HasForeignKey(c => c.ResponsiblePositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}