using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Jobs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedNever();

        // Unique & Lookup Indexes
        builder.HasIndex(j => j.ReferenceId).IsUnique();
        builder.HasIndex(j => j.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(j => new { j.IsActive, j.JobFamily, j.Code }).HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(j => j.DefaultJobGradeId);

        // Column Constraints (Single Source of Truth from JobRules)
        builder.Property(j => j.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(j => j.Code).HasMaxLength(JobRules.MaxCodeLength).IsRequired();
        builder.Property(j => j.Name).HasMaxLength(JobRules.MaxNameLength).IsRequired();
        builder.Property(j => j.InternationalName).HasMaxLength(JobRules.MaxNameLength).IsRequired();
        builder.Property(j => j.JobFamily).HasMaxLength(JobRules.MaxJobFamilyLength);
        builder.Property(j => j.Description).HasMaxLength(JobRules.MaxDescriptionLength);

        // Relationships
        builder.HasOne(j => j.DefaultJobGrade)
            .WithMany()
            .HasForeignKey(j => j.DefaultJobGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}