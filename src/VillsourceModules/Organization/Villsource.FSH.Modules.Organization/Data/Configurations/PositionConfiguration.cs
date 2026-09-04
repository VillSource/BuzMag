using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Data.Configurations;

internal sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(36);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(36);
        builder.Property(x => x.DeletedBy).HasMaxLength(36);
        
        builder.Property(x=>x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x=>x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(150);
        
        
        builder.HasIndex( nameof(Position.ReferenceId)).IsUnique();
        builder.HasIndex( nameof(Position.Code)).IsUnique();
        
        builder.Property(x=>x.PositionTiers)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v.Select(t => t.Key)),
                v => SafeDeserialize(v)
            );

        builder.Ignore(x => x.DomainEvents);
    }
    
    private static PositionTier[] SafeDeserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            string[]? values = JsonSerializer.Deserialize<string[]>(json);

            if (values == null || values.Length == 0)
            {
                return [];
            }

            return values
                .Select(val => PositionTier.TryGet(val, out PositionTier? tier) ? tier : null)
                .Where(tier => tier != null)
                .Cast<PositionTier>()
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}