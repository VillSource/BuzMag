using FSH.Framework.Core.Domain;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;

public sealed partial class Employment: BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public Guid EmployeeId { get; private set; }

    public EmploymentType Type { get; private set; } = EmploymentType.None;
    public EmploymentStatus Status { get; private set; } = EmploymentStatus.None;
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; set; }

    public string? Note { get; private set; } 


    public static Employment Create(Guid employmentId, EmploymentType type, DateTimeOffset effectiveDate)
    {
        ArgumentNullException.ThrowIfNull(type);
        
        return new Employment
        {
            EmployeeId = employmentId,
            Type = type,
            EffectiveFrom = effectiveDate,
            Status = EmploymentStatus.NewHire,
        };
    }

    public void SetNote(string? note) => Note = note;

    public void SetStatus(EmploymentStatus status)
    {
        Status = status;
    }
}