using System.Text.Json.Serialization;

namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class PositionAllocationDto
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public PositionDto Position { get; set; } = new();
    public int? HeadCount { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
