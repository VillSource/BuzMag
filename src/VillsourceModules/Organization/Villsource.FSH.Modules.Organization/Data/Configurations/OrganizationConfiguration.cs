using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Domain.Organization>
{
    public void Configure(EntityTypeBuilder<Domain.Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedBy).HasMaxLength(36);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);
        builder.Property(x => x.IsDefault).HasDefaultValue(false);
        
        builder.HasMany(x=>x.Units).WithOne().HasForeignKey(x => x.OrganizationId);
        
        builder.Ignore(x => x.DomainEvents);
    }
}