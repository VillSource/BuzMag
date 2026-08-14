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
        builder.Property(x => x.CreatedBy).HasMaxLength(36);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);

        builder.Property(x => x.EffectiveFrom).IsRequired();

        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId);
        builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId);

        builder.Ignore(x => x.DomainEvents);
    }
}