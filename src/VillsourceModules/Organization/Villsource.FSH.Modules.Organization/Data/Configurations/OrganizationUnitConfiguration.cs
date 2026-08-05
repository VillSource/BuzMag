using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;


internal sealed class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.ToTable("OrganizationUnits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedBy).HasMaxLength(36).IsRequired();
        builder.Property(x => x.ReferenceId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);
        
        builder.Property(x=>x.Path).HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x=>x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(150);
        
        builder.HasMany(x=>x.Children).WithOne().HasForeignKey(x=>x.ParenId);
        builder.HasMany(x=>x.Positions).WithOne();

        builder.Ignore(x => x.DomainEvents);
    }
}