using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Data.Configurations;

public class EmploymentConfiguration : IEntityTypeConfiguration<Employment>
{
    public void Configure(EntityTypeBuilder<Employment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Employments");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Ref).IsUnique();
        builder.Property(e => e.Ref).HasMaxLength(VillsourceId.KeySizes).IsRequired();

        builder.Property(e => e.EmployeeId).IsRequired();
        builder.Property(e => e.StartNote).HasMaxLength(500);
        builder.Property(e => e.EndNote).HasMaxLength(500);

        builder.Property(e => e.Type)
            .HasConversion(
                type => type.Key,
                key => ToEmploymentType(key))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                status => status.Key,
                key => ToEmploymentStatus(key))
            .HasMaxLength(50)
            .IsRequired();
    }

    private static EmploymentType ToEmploymentType(string? key) 
        => EmploymentType.TryGet(key, out EmploymentType? type) ? type : EmploymentType.None;

    private static EmploymentStatus ToEmploymentStatus(string? key) 
        => EmploymentStatus.TryGet(key, out EmploymentStatus? status) ? status : EmploymentStatus.None;
}