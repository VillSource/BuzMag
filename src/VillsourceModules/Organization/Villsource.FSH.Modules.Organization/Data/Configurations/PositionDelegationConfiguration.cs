using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

public sealed class PositionDelegationConfiguration : IEntityTypeConfiguration<PositionDelegation>
{
    public void Configure(EntityTypeBuilder<PositionDelegation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PositionDelegations");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        // Unique & Partial Active-Window Indexes for fast approval resolution
        builder.HasIndex(d => d.ReferenceId).IsUnique();
        builder.HasIndex(d => new { d.DelegatorPositionId, d.ValidFrom, d.ValidTo })
            .HasFilter("\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");
        builder.HasIndex(d => new { d.DelegatePositionId, d.ValidFrom, d.ValidTo })
            .HasFilter("\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");
        builder.HasIndex(d => new { d.LegalEntityId, d.IsRevoked, d.ValidFrom });

        // Column Constraints (Single Source of Truth from PositionDelegationRules)
        builder.Property(d => d.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(d => d.DocumentTypeScope).HasMaxLength(PositionDelegationRules.MaxDocumentTypeScopeLength);
        builder.Property(d => d.Reason).HasMaxLength(PositionDelegationRules.MaxReasonLength);
        builder.Property(d => d.RevokedBy).HasMaxLength(PositionDelegationRules.MaxRevokedByLength);
        builder.Property(d => d.RevocationReason).HasMaxLength(PositionDelegationRules.MaxReasonLength);

        builder.Property(d => d.DelegationType)
            .HasConversion(
                v => v.Key,
                v => DelegationType.Get(v))
            .HasMaxLength(PositionDelegationRules.MaxDelegationTypeLength)
            .IsRequired();

        // Relationships
        builder.HasOne(d => d.LegalEntity)
            .WithMany()
            .HasForeignKey(d => d.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DelegatorPosition)
            .WithMany()
            .HasForeignKey(d => d.DelegatorPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DelegatePosition)
            .WithMany()
            .HasForeignKey(d => d.DelegatePositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}