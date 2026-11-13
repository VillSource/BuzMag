using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public class LegalEntityConfiguration : IEntityTypeConfiguration<LegalEntity>
{
    public void Configure(EntityTypeBuilder<LegalEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LegalEntities");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.ReferenceId).IsUnique();
        builder.HasIndex(e => e.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(e => new { e.CountryCode, e.TaxId });

        builder.Property(e => e.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(e => e.LegalName).HasMaxLength(250).IsRequired();
        builder.Property(e => e.InternationalName).HasMaxLength(250).IsRequired();
        builder.Property(e => e.TaxId).HasMaxLength(30).IsRequired();
        builder.Property(e => e.RegistrationNumber).HasMaxLength(50);
        builder.Property(e => e.BaseCurrency).HasMaxLength(3).IsRequired();

        builder.ComplexProperty(e => e.Profile, profile =>
        {
            profile.Property(p => p.TradeName).HasMaxLength(250);
            profile.Property(p => p.LogoUrl).HasMaxLength(500);
            profile.Property(p => p.Website).HasMaxLength(250);
            profile.Property(p => p.ContactEmail).HasMaxLength(256);
            profile.Property(p => p.ContactPhone).HasMaxLength(50);
        });

        builder.HasMany(e => e.Branches)
            .WithOne()
            .HasForeignKey(b => b.LegalEntityId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(e => e.Branches)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}