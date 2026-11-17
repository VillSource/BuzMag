using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class JobFamilyConfiguration : IEntityTypeConfiguration<JobFamily>
{
    public void Configure(EntityTypeBuilder<JobFamily> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("JobFamilies");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.HasIndex(f => f.ReferenceId).IsUnique();
        builder.HasIndex(f => f.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(f => new { f.IsActive, f.Code }).HasFilter("\"IsDeleted\" = FALSE");

        builder.Property(f => f.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(f => f.Code).HasMaxLength(JobFamilyRules.MaxCodeLength).IsRequired();
        builder.Property(f => f.Name).HasMaxLength(JobFamilyRules.MaxNameLength).IsRequired();
        builder.Property(f => f.InternationalName).HasMaxLength(JobFamilyRules.MaxNameLength).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(JobFamilyRules.MaxDescriptionLength);
    }
}
