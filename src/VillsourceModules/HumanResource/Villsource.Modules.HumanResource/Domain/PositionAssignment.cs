using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;

public sealed partial class PositionAssignment : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public Guid EmployeeId { get; private set; }
    public Guid? ManagerId { get; private set; }
    public string OrganizationUnitRef { get; private set; } = string.Empty;
    public string PositionRef { get; private set; } = string.Empty;
    public PositionTier PositionTier { get; private set; } = PositionTier.None;

    public PositionAssignmentType Type { get; private set; } = PositionAssignmentType.None;
    public bool IsPrimary { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }

    public Employee Employee { get; } = null!;
    public Employee? Manager { get; }

    private PositionAssignment() { }

    public static PositionAssignment Create(Guid employeeId, string organizationUnitRef, string positionRef,
        PositionTier positionTier, PositionAssignmentType type, bool isPrimary, DateTimeOffset effectiveDate)
    {
        return new PositionAssignment
        {
            EmployeeId = employeeId,
            OrganizationUnitRef = organizationUnitRef,
            PositionRef = positionRef,
            PositionTier = positionTier,
            Type = type,
            IsPrimary = isPrimary,
            EffectiveFrom = effectiveDate,
        };
    }

    public void AssignManager(Employee? manager) => ManagerId = manager?.Id;
    public void SetEndEffectiveDate(DateTimeOffset endEffectiveAt) => EffectiveTo = endEffectiveAt;
}