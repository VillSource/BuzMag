using FSH.Framework.Core.Domain;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;

public sealed partial class Employment: BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public Guid EmployeeId { get; set; }

    public EmploymentType Type { get; set; } = EmploymentType.None;
    public EmploymentStatus Status { get; set; } = EmploymentStatus.None;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }

    public string? StartNote { get; set; } 
    public string? EndNote { get; set; } 
}