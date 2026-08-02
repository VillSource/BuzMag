using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

internal sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedBy).HasMaxLength(32).IsRequired();
        builder.Property(x => x.LastModifiedBy).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DeletedBy).HasMaxLength(32).IsRequired();
        
        builder.Property(x=>x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x=>x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(150);

        builder.Ignore(x => x.DomainEvents);
    }
}