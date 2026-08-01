using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Domain.Organization>
{
    public void Configure(EntityTypeBuilder<Domain.Organization> builder)
    {
        builder.ToTable("Organization");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder.HasMany(x=>x.Units).WithOne();

        builder.Ignore(x => x.DomainEvents);
    }
}