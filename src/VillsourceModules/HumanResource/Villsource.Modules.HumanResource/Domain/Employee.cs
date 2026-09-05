using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.ObjectValue;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;


public sealed partial class Employee: AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public string? UserId { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    
    // personal information
    public string Title { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } 
    public string LastName { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string? MiddleNameEn { get; set; } 
    public string LastNameEn { get; set; } = string.Empty;
    public Address Address { get; set; } = Address.Empty();
    public string Email { get; set; } = string.Empty;
    
    
    // job label
    public bool IsActive { get; set; }
    public DateTimeOffset LastHireDate { get; set; }
    public DateTimeOffset FirstHireDate { get; set; }
    public string? SnapshotOuName { get; set; } 
    public string? SnapshotOuId { get; set; } 
    public string? SnapshotPositionName { get; set; } 
    public string? SnapshotPositionId { get; set; } 
    public PositionTier? SnapshotTier { get; set; } 

    private readonly List<Employment> _employments = [];
    public IReadOnlyCollection<Employment> Employments  => _employments.AsReadOnly();
    
    private readonly List<PositionAssignment> _positionAssignments = [];
    public IReadOnlyCollection<PositionAssignment> PositionAssignments => _positionAssignments.AsReadOnly();
}