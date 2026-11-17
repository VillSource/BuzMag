using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations
{
    public sealed class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
    {
        public void Configure(EntityTypeBuilder<JobGrade> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("JobGrades");
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id).ValueGeneratedNever();

            // Unique & Lookup Indexes
            builder.HasIndex(g => g.ReferenceId).IsUnique();
            builder.HasIndex(g => g.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(g => new { g.IsActive, g.RankOrder, g.Code }).HasFilter("\"IsDeleted\" = FALSE");

            // Column Constraints (Single Source of Truth from JobGradeRules)
            builder.Property(g => g.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(g => g.Code).HasMaxLength(JobGradeRules.MaxCodeLength).IsRequired();
            builder.Property(g => g.Name).HasMaxLength(JobGradeRules.MaxNameLength).IsRequired();
            builder.Property(g => g.InternationalName).HasMaxLength(JobGradeRules.MaxNameLength).IsRequired();
            builder.Property(g => g.JobLevelCategory)
                .HasConversion(
                    v => v == null ? null : v.Key,
                    v => v == null ? null : JobLevelCategory.Get(v))
                .HasMaxLength(JobGradeRules.MaxCategoryLength);
            builder.Property(g => g.Description).HasMaxLength(JobGradeRules.MaxDescriptionLength);
        }
    }
}
