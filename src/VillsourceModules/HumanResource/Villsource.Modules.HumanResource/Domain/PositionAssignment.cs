using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;

public sealed partial class PositionAssignment: BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public Guid EmployeeId { get; set; }
    public Guid? ManagerId { get; set; }
    public string OrganizationUnitRef { get; set; } = string.Empty;
    public string PositionRef { get; set; } = string.Empty;
    public PositionTier PositionTier { get; set; } = PositionTier.None;

    public PositionAssignmentType Type { get; set; } = PositionAssignmentType.None;
    public bool IsPrimary { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }

    public Employee? Manager { get; }
}