using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.DocumentTypes;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("DocumentTypes");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.HasIndex(d => d.Code).IsUnique();
        builder.HasIndex(d => d.ModuleName);

        builder.Property(d => d.Code).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.ModuleName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);
    }
}
