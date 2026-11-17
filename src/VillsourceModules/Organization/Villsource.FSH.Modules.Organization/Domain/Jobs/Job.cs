using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.Jobs;

public sealed partial class Job : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private Job() { }

    public string ReferenceId { get; } = VillsourceId.Key;
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string? JobFamily { get; private set; }
    public string? Description { get; private set; }

    public Guid DefaultJobGradeId { get; private set; }
    public JobGrade? DefaultJobGrade { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public static Job Create(
        string code,
        string name,
        string internationalName,
        Guid defaultJobGradeId,
        string? jobFamily = null,
        string? description = null,
        bool isActive = true)
    {
        var (normalizedCode, normalizedName, normalizedIntlName, normalizedFamily, normalizedDesc) =
            JobRules.NormalizeAndValidate(code, name, internationalName, defaultJobGradeId, jobFamily, description);

        var entity = new Job
        {
            Id = Guid.CreateVersion7(),
            Code = normalizedCode,
            Name = normalizedName,
            InternationalName = normalizedIntlName,
            DefaultJobGradeId = defaultJobGradeId,
            JobFamily = normalizedFamily,
            Description = normalizedDesc,
            IsActive = isActive
        };

        entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobCreatedDomainEvent(
                JobId: entity.Id,
                Code: entity.Code,
                Name: entity.Name,
                DefaultJobGradeId: entity.DefaultJobGradeId,
                EventId: id,
                OccurredOnUtc: ts)));

        return entity;
    }

    /// <summary>
    /// Updates mutable attributes. Returns <c>true</c> if any field changed;
    /// returns <c>false</c> on no-op updates to skip redundant DB writes and events.
    /// </summary>
    public bool Update(
        string code,
        string name,
        string internationalName,
        Guid defaultJobGradeId,
        string? jobFamily = null,
        string? description = null)
    {
        EnsureNotDeleted();

        var (normalizedCode, normalizedName, normalizedIntlName, normalizedFamily, normalizedDesc) =
            JobRules.NormalizeAndValidate(code, name, internationalName, defaultJobGradeId, jobFamily, description);

        if (string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
            string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
            string.Equals(InternationalName, normalizedIntlName, StringComparison.Ordinal) &&
            DefaultJobGradeId == defaultJobGradeId &&
            string.Equals(JobFamily, normalizedFamily, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDesc, StringComparison.Ordinal))
        {
            return false;
        }

        Code = normalizedCode;
        Name = normalizedName;
        InternationalName = normalizedIntlName;
        DefaultJobGradeId = defaultJobGradeId;
        JobFamily = normalizedFamily;
        Description = normalizedDesc;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobUpdatedDomainEvent(
                JobId: Id,
                Code: Code,
                Name: Name,
                DefaultJobGradeId: DefaultJobGradeId,
                EventId: id,
                OccurredOnUtc: ts)));

        return true;
    }

    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobActivatedDomainEvent(JobId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobDeactivatedDomainEvent(JobId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobDeletedDomainEvent(JobId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Job '{Id}' has already been deleted.");
        }
    }
}