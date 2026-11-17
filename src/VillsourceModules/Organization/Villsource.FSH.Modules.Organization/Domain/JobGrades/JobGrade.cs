using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.JobGrades.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.JobGrades
{
    public sealed partial class JobGrade : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
    {
        private JobGrade() { }

        public string ReferenceId { get; } = VillsourceId.Key;
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string InternationalName { get; private set; } = null!;
        public int RankOrder { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        public static JobGrade Create(
            string code,
            string name,
            string internationalName,
            int rankOrder,
            string? description = null,
            bool isActive = true)
        {
            var (normalizedCode, normalizedName, normalizedIntlName, normalizedDesc) =
                JobGradeRules.NormalizeAndValidate(code, name, internationalName, rankOrder, description);

            var entity = new JobGrade
            {
                Id = Guid.CreateVersion7(),
                Code = normalizedCode,
                Name = normalizedName,
                InternationalName = normalizedIntlName,
                RankOrder = rankOrder,
                Description = normalizedDesc,
                IsActive = isActive
            };

            entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
                new JobGradeCreatedDomainEvent(
                    JobGradeId: entity.Id,
                    Code: entity.Code,
                    Name: entity.Name,
                    RankOrder: entity.RankOrder,
                    EventId: id,
                    OccurredOnUtc: ts)));

            return entity;
        }

        /// <summary>
        /// Updates mutable attributes. Returns <c>true</c> if any state actually changed;
        /// returns <c>false</c> when values are identical to prevent redundant DB updates and domain events.
        /// </summary>
        public bool Update(
            string code,
            string name,
            string internationalName,
            int rankOrder,
            string? description = null)
        {
            EnsureNotDeleted();

            var (normalizedCode, normalizedName, normalizedIntlName, normalizedDesc) =
                JobGradeRules.NormalizeAndValidate(code, name, internationalName, rankOrder, description);

            if (string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
                string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
                string.Equals(InternationalName, normalizedIntlName, StringComparison.Ordinal) &&
                RankOrder == rankOrder &&
                string.Equals(Description, normalizedDesc, StringComparison.Ordinal))
            {
                return false;
            }

            Code = normalizedCode;
            Name = normalizedName;
            InternationalName = normalizedIntlName;
            RankOrder = rankOrder;
            Description = normalizedDesc;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new JobGradeUpdatedDomainEvent(
                    JobGradeId: Id,
                    Code: Code,
                    Name: Name,
                    RankOrder: RankOrder,
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
                new JobGradeActivatedDomainEvent(JobGradeId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        public void Deactivate()
        {
            EnsureNotDeleted();
            if (!IsActive) return;

            IsActive = false;
            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new JobGradeDeactivatedDomainEvent(JobGradeId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        public void Delete(string? deletedBy = null)
        {
            if (IsDeleted) return;

            DeletedOnUtc = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
            IsDeleted = true;
            IsActive = false;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new JobGradeDeletedDomainEvent(JobGradeId: Id, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException($"Job grade '{Id}' has already been deleted.");
            }
        }
    }
}
