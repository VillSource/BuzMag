using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

internal sealed class OrganizationUnitPositionAllocationConfiguration : IEntityTypeConfiguration<OrganizationUnitPositionAllocation>
{
    public void Configure(EntityTypeBuilder<OrganizationUnitPositionAllocation> builder)
    {
        builder.ToTable("OrganizationUnitPositionAllocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(36);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);

        builder.Property(x => x.EffectiveFrom).IsRequired();
        builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId })
            .HasFilter("\"EffectiveTo\" IS NULL AND NOT \"IsDeleted\"");

        builder.HasOne(x => x.Unit).WithMany(x => x.PositionAllocations).HasForeignKey(x => x.OrganizationUnitId).IsRequired();
        builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}
