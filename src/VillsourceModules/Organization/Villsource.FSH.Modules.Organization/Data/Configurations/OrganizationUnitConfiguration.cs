using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("OrganizationUnits");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.HasIndex(e => e.ReferenceId).IsUnique();

        builder.HasIndex(e => e.ParentId);
        builder.HasIndex(e => e.LegalEntityId);
        builder.HasIndex(e => e.ManagerPositionId);
        builder.HasIndex(e => e.HierarchyPath);

        builder.Property(e => e.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(250).IsRequired();
        builder.Property(e => e.InternationalName).HasMaxLength(250).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.LegalEntityRefId).HasMaxLength(VillsourceId.KeySizes);
        builder.Property(e => e.HierarchyPath).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.CostCenterCode).HasMaxLength(50);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.ManagerPosition)
            .WithMany()
            .HasForeignKey(e => e.ManagerPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(e => e.Children)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}