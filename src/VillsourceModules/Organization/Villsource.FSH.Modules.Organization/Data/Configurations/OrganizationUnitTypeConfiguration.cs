using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class OrganizationUnitTypeConfiguration : IEntityTypeConfiguration<OrganizationUnitType>
{
    public void Configure(EntityTypeBuilder<OrganizationUnitType> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("OrganizationUnitTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.HasIndex(t => t.ReferenceId).IsUnique();
        builder.HasIndex(t => t.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(t => new { t.IsActive, t.HierarchyLevel, t.Code }).HasFilter("\"IsDeleted\" = FALSE");

        builder.Property(t => t.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(t => t.Code).HasMaxLength(OrganizationUnitTypeRules.MaxCodeLength).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(OrganizationUnitTypeRules.MaxNameLength).IsRequired();
        builder.Property(t => t.InternationalName).HasMaxLength(OrganizationUnitTypeRules.MaxNameLength).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(OrganizationUnitTypeRules.MaxDescriptionLength);
    }
}
