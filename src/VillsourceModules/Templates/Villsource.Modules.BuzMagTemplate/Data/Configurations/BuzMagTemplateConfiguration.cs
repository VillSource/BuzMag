using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.Modules.BuzMagTemplate.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.BuzMagTemplate.Data.Configurations;

internal sealed class BuzMagTemplateConfiguration : IEntityTypeConfiguration<Ping>
{
    public void Configure(EntityTypeBuilder<Ping> builder)
    {
        builder.ToTable("OrganizationUnits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Ref).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(36);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);

        builder.Ignore(x => x.DomainEvents);
    }
}