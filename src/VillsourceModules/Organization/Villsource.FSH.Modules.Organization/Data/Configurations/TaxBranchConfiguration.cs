using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public class TaxBranchConfiguration : IEntityTypeConfiguration<TaxBranch>
{
    public void Configure(EntityTypeBuilder<TaxBranch> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("TaxBranches");
        builder.HasKey(b => b.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.HasIndex(b => b.ReferenceId).IsUnique();
        
        builder.HasIndex(b => new { b.LegalEntityId, b.BranchCode }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(b => b.LegalEntityId)
            .HasDatabaseName("IX_TaxBranches_LegalEntityId_HeadOffice")
            .IsUnique()
            .HasFilter($"\"{nameof(TaxBranch.IsHeadOffice)}\" = TRUE AND \"IsDeleted\" = FALSE");

        builder.Property(b => b.ReferenceId).IsRequired();
        builder.Property(b => b.LegalEntityId).IsRequired();
        builder.Property(b => b.BranchCode).HasMaxLength(20).IsRequired();
        builder.Property(b => b.IsHeadOffice).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(250).IsRequired();
        builder.Property(b => b.InternationalName).HasMaxLength(250).IsRequired();
        builder.Property(b => b.TaxSubId).HasMaxLength(30);

        builder.ComplexProperty(b => b.RegisteredAddress).ConfigureAddress();

        builder.Property(b => b.IsActive).IsRequired();
        builder.Property(b => b.RegisteredDate);
        builder.Property(b => b.ClosedDate);
    }
}