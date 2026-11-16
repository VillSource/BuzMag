using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Domain;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using FSH.Framework.Persistence.Specifications;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.v2.Positions;
using Villsource.FSH.Modules.Organization.Domain.v2.Positions.Events;
using Villsource.Tool.UniqueKey;

// =============================================================================
// 1. DOMAIN LAYER (Rules, Aggregate Root, Domain Events)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Domain.v2.Positions
{
    internal static partial class PositionRules
    {
        public const int MaxCodeLength = 50;
        public const int MaxTitleLength = 200;
        public const int MinHeadcountCapacity = 1;

        [GeneratedRegex(@"^[A-Z0-9\-_]{2,50}$", RegexOptions.CultureInvariant)]
        private static partial Regex CodeRegex();

        public static bool IsValidCode(string? code) =>
            !string.IsNullOrWhiteSpace(code) && CodeRegex().IsMatch(code.Trim().ToUpperInvariant());

        public static string NormalizeAndValidateCode(string code)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            var normalized = code.Trim().ToUpperInvariant();

            if (!CodeRegex().IsMatch(normalized))
            {
                throw new ArgumentException(
                    $"Position code '{normalized}' must be 2-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                    nameof(code));
            }

            return normalized;
        }

        public static (DateTimeOffset? NormalizedFrom, DateTimeOffset? NormalizedTo) NormalizeAndValidateValidity(
            DateTimeOffset? validFrom,
            DateTimeOffset? validTo)
        {
            var utcFrom = validFrom?.ToUniversalTime();
            var utcTo = validTo?.ToUniversalTime();

            if (utcFrom.HasValue && utcTo.HasValue && utcTo.Value < utcFrom.Value)
            {
                throw new ArgumentException("ValidTo cannot be earlier than ValidFrom.", nameof(validTo));
            }

            return (utcFrom, utcTo);
        }

        public static void ValidateReportingTargets(
            Guid positionId,
            Guid? reportsToPositionId,
            Guid? dottedLinePositionId)
        {
            if (reportsToPositionId.HasValue && reportsToPositionId.Value == positionId)
            {
                throw new InvalidOperationException("A position cannot report to itself.");
            }

            if (dottedLinePositionId.HasValue && dottedLinePositionId.Value == positionId)
            {
                throw new InvalidOperationException("A position cannot have a dotted-line report to itself.");
            }

            if (reportsToPositionId.HasValue &&
                dottedLinePositionId.HasValue &&
                reportsToPositionId.Value == dottedLinePositionId.Value)
            {
                throw new InvalidOperationException(
                    "Dotted-line reporting position cannot be the same as the primary reporting position.");
            }
        }
    }

    public sealed partial class Position : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
    {
        private readonly List<Position> _directReports = [];

        private Position() { }

        // 1. Identity & Organizational Placement
        public string ReferenceId { get; } = VillsourceId.Key;
        public Guid LegalEntityId { get; private set; }
        public LegalEntity? LegalEntity { get; private set; } = null!;

        public Guid OrgUnitId { get; private set; }

        public Guid JobId { get; private set; }
        public Job? Job { get; private set; } = null!;

        public Guid JobGradeId { get; private set; }
        public JobGrade? JobGrade { get; private set; } = null!;

        public Guid? CostCenterId { get; private set; }
        public CostCenter? CostCenter { get; private set; } = null!;

        // 2. Reporting Lines (Chain of Command)
        public Guid? ReportsToPositionId { get; private set; }
        public Position? ReportsToPosition { get; private set; } = null!;
        public IReadOnlyCollection<Position> DirectReports => _directReports.AsReadOnly();

        public Guid? DottedLinePositionId { get; private set; }
        public Position? DottedLinePosition { get; private set; } = null!;

        // 3. Position Attributes & Headcount
        public string Code { get; private set; } = null!;
        public string Title { get; private set; } = null!;
        public string InternationalTitle { get; private set; } = null!;
        public int HeadcountCapacity { get; private set; } = 1;
        public bool IsPrimaryLeadership { get; private set; }

        // 4. Validity & Lifecycle
        public DateTimeOffset? ValidFrom { get; private set; }
        public DateTimeOffset? ValidTo { get; private set; }
        public bool IsActive { get; private set; } = true;

        public static Position Create(
            Guid legalEntityId,
            Guid orgUnitId,
            Guid jobId,
            Guid jobGradeId,
            string code,
            string title,
            string internationalTitle,
            Guid? costCenterId = null,
            Guid? reportsToPositionId = null,
            Guid? dottedLinePositionId = null,
            int headcountCapacity = 1,
            bool isPrimaryLeadership = false,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null,
            bool isActive = true)
        {
            if (legalEntityId == Guid.Empty) throw new ArgumentException("LegalEntityId cannot be empty.", nameof(legalEntityId));
            if (orgUnitId == Guid.Empty) throw new ArgumentException("OrgUnitId cannot be empty.", nameof(orgUnitId));
            if (jobId == Guid.Empty) throw new ArgumentException("JobId cannot be empty.", nameof(jobId));
            if (jobGradeId == Guid.Empty) throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));

            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalTitle);

            if (headcountCapacity < PositionRules.MinHeadcountCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(headcountCapacity),
                    $"HeadcountCapacity must be at least {PositionRules.MinHeadcountCapacity}.");
            }

            var id = Guid.CreateVersion7();
            PositionRules.ValidateReportingTargets(id, reportsToPositionId, dottedLinePositionId);

            var normalizedCode = PositionRules.NormalizeAndValidateCode(code);
            var (utcFrom, utcTo) = PositionRules.NormalizeAndValidateValidity(validFrom, validTo);

            var entity = new Position
            {
                Id = id,
                LegalEntityId = legalEntityId,
                OrgUnitId = orgUnitId,
                JobId = jobId,
                JobGradeId = jobGradeId,
                CostCenterId = costCenterId,
                ReportsToPositionId = reportsToPositionId,
                DottedLinePositionId = dottedLinePositionId,
                Code = normalizedCode,
                Title = title.Trim(),
                InternationalTitle = internationalTitle.Trim(),
                HeadcountCapacity = headcountCapacity,
                IsPrimaryLeadership = isPrimaryLeadership,
                ValidFrom = utcFrom,
                ValidTo = utcTo,
                IsActive = isActive
            };

            entity.AddDomainEvent(DomainEvent.Create((eventId, ts) =>
                new PositionCreatedDomainEvent(
                    PositionId: entity.Id,
                    LegalEntityId: entity.LegalEntityId,
                    OrgUnitId: entity.OrgUnitId,
                    JobId: entity.JobId,
                    JobGradeId: entity.JobGradeId,
                    ReportsToPositionId: entity.ReportsToPositionId,
                    Code: entity.Code,
                    Title: entity.Title,
                    IsPrimaryLeadership: entity.IsPrimaryLeadership,
                    EventId: eventId,
                    OccurredOnUtc: ts)));

            return entity;
        }

        /// <summary>
        /// Updates mutable attributes. Returns <c>true</c> if any property changed;
        /// returns <c>false</c> when identical to avoid redundant DB updates and domain events.
        /// </summary>
        public bool Update(
            Guid orgUnitId,
            Guid jobId,
            Guid jobGradeId,
            string code,
            string title,
            string internationalTitle,
            Guid? costCenterId = null,
            Guid? reportsToPositionId = null,
            Guid? dottedLinePositionId = null,
            int headcountCapacity = 1,
            bool isPrimaryLeadership = false,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null)
        {
            EnsureNotDeleted();

            if (orgUnitId == Guid.Empty) throw new ArgumentException("OrgUnitId cannot be empty.", nameof(orgUnitId));
            if (jobId == Guid.Empty) throw new ArgumentException("JobId cannot be empty.", nameof(jobId));
            if (jobGradeId == Guid.Empty) throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));

            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalTitle);

            if (headcountCapacity < PositionRules.MinHeadcountCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(headcountCapacity),
                    $"HeadcountCapacity must be at least {PositionRules.MinHeadcountCapacity}.");
            }

            PositionRules.ValidateReportingTargets(Id, reportsToPositionId, dottedLinePositionId);

            var normalizedCode = PositionRules.NormalizeAndValidateCode(code);
            var normalizedTitle = title.Trim();
            var normalizedIntlTitle = internationalTitle.Trim();
            var (utcFrom, utcTo) = PositionRules.NormalizeAndValidateValidity(validFrom, validTo);

            if (OrgUnitId == orgUnitId &&
                JobId == jobId &&
                JobGradeId == jobGradeId &&
                CostCenterId == costCenterId &&
                ReportsToPositionId == reportsToPositionId &&
                DottedLinePositionId == dottedLinePositionId &&
                string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
                string.Equals(Title, normalizedTitle, StringComparison.Ordinal) &&
                string.Equals(InternationalTitle, normalizedIntlTitle, StringComparison.Ordinal) &&
                HeadcountCapacity == headcountCapacity &&
                IsPrimaryLeadership == isPrimaryLeadership &&
                ValidFrom == utcFrom &&
                ValidTo == utcTo)
            {
                return false;
            }

            OrgUnitId = orgUnitId;
            JobId = jobId;
            JobGradeId = jobGradeId;
            CostCenterId = costCenterId;
            ReportsToPositionId = reportsToPositionId;
            DottedLinePositionId = dottedLinePositionId;
            Code = normalizedCode;
            Title = normalizedTitle;
            InternationalTitle = normalizedIntlTitle;
            HeadcountCapacity = headcountCapacity;
            IsPrimaryLeadership = isPrimaryLeadership;
            ValidFrom = utcFrom;
            ValidTo = utcTo;

            AddDomainEvent(DomainEvent.Create((eventId, ts) =>
                new PositionUpdatedDomainEvent(
                    PositionId: Id,
                    LegalEntityId: LegalEntityId,
                    OrgUnitId: OrgUnitId,
                    JobId: JobId,
                    JobGradeId: JobGradeId,
                    ReportsToPositionId: ReportsToPositionId,
                    Code: Code,
                    Title: Title,
                    IsPrimaryLeadership: IsPrimaryLeadership,
                    EventId: eventId,
                    OccurredOnUtc: ts)));

            return true;
        }

        public void Activate()
        {
            EnsureNotDeleted();
            if (IsActive) return;

            if (ReportsToPosition is { IsActive: false } or { IsDeleted: true })
            {
                throw new InvalidOperationException(
                    "Cannot activate a position while its reporting manager position is inactive or deleted.");
            }

            IsActive = true;
            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionActivatedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        public void Deactivate()
        {
            EnsureNotDeleted();
            if (!IsActive) return;

            if (_directReports.Any(p => !p.IsDeleted && p.IsActive))
            {
                throw new InvalidOperationException(
                    "Cannot deactivate a position while active direct report positions still exist.");
            }

            IsActive = false;
            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDeactivatedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        public void Delete(string? deletedBy = null)
        {
            if (IsDeleted) return;

            if (_directReports.Any(p => !p.IsDeleted))
            {
                throw new InvalidOperationException(
                    "Cannot delete a position while direct report positions still exist.");
            }

            DeletedOnUtc = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
            IsDeleted = true;
            IsActive = false;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDeletedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException($"Position '{Id}' has already been deleted.");
            }
        }
    }
}

namespace Villsource.FSH.Modules.Organization.Domain.v2.Positions.Events
{
    public sealed record PositionCreatedDomainEvent(
        Guid PositionId,
        Guid LegalEntityId,
        Guid OrgUnitId,
        Guid JobId,
        Guid JobGradeId,
        Guid? ReportsToPositionId,
        string Code,
        string Title,
        bool IsPrimaryLeadership,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionUpdatedDomainEvent(
        Guid PositionId,
        Guid LegalEntityId,
        Guid OrgUnitId,
        Guid JobId,
        Guid JobGradeId,
        Guid? ReportsToPositionId,
        string Code,
        string Title,
        bool IsPrimaryLeadership,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionActivatedDomainEvent(
        Guid PositionId,
        Guid LegalEntityId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionDeactivatedDomainEvent(
        Guid PositionId,
        Guid LegalEntityId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionDeletedDomainEvent(
        Guid PositionId,
        Guid LegalEntityId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}

// =============================================================================
// 2. PERSISTENCE LAYER (EF Core Configuration)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Data.v2.Configurations
{
    public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("PositionsV2");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            // Unique & Hierarchy Lookup Indexes
            builder.HasIndex(p => p.ReferenceId).IsUnique();
            builder.HasIndex(p => new { p.LegalEntityId, p.Code })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(p => new { p.LegalEntityId, p.OrgUnitId, p.IsActive })
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(p => new { p.LegalEntityId, p.ReportsToPositionId })
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(p => p.JobGradeId);
            builder.HasIndex(p => p.JobId);
            builder.HasIndex(p => p.CostCenterId);

            // Column Constraints (Single Source of Truth from PositionRules)
            builder.Property(p => p.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(p => p.Code).HasMaxLength(PositionRules.MaxCodeLength).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(PositionRules.MaxTitleLength).IsRequired();
            builder.Property(p => p.InternationalTitle).HasMaxLength(PositionRules.MaxTitleLength).IsRequired();

            // Relationships
            builder.HasOne(p => p.LegalEntity)
                .WithMany()
                .HasForeignKey(p => p.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Job)
                .WithMany()
                .HasForeignKey(p => p.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.JobGrade)
                .WithMany()
                .HasForeignKey(p => p.JobGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.CostCenter)
                .WithMany()
                .HasForeignKey(p => p.CostCenterId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.ReportsToPosition)
                .WithMany(p => p.DirectReports)
                .HasForeignKey(p => p.ReportsToPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(p => p.DirectReports)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(p => p.DottedLinePosition)
                .WithMany()
                .HasForeignKey(p => p.DottedLinePositionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// =============================================================================
// 3. APPLICATION & API LAYER (Mappers, Specs, Validators, Handlers, Endpoints)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Features.v2.Positions
{
    #region Mapperly Mappers

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class PositionMapper
    {
        [MapProperty(nameof(Position.ReferenceId), nameof(PositionDetailDto.RefId))]
        [MapProperty([nameof(Position.LegalEntity), nameof(LegalEntity.ReferenceId)], nameof(PositionDetailDto.LegalEntityRefId))]
        [MapProperty([nameof(Position.LegalEntity), nameof(LegalEntity.Code)], nameof(PositionDetailDto.LegalEntityCode))]
        [MapProperty([nameof(Position.LegalEntity), nameof(LegalEntity.LegalName)], nameof(PositionDetailDto.LegalEntityName))]
        [MapPropertyFromSource(nameof(PositionDetailDto.DirectReports), Use = nameof(MapDirectReports))]
        [MapPropertyFromSource(nameof(PositionDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
        [MapPropertyFromSource(nameof(PositionDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
        public static partial PositionDetailDto ToDetailDto(this Position entity);

        [MapProperty(nameof(Position.ReferenceId), nameof(PositionNodeDto.RefId))]
        [MapProperty([nameof(Position.JobGrade), nameof(JobGrade.Code)], nameof(PositionNodeDto.JobGradeCode))]
        [MapProperty([nameof(Position.JobGrade), nameof(JobGrade.RankOrder)], nameof(PositionNodeDto.JobGradeRankOrder))]
        public static partial PositionNodeDto ToNodeDto(this Position entity);

        [MapProperty(nameof(Job.ReferenceId), nameof(PositionJobDto.RefId))]
        public static partial PositionJobDto ToJobDto(this Job job);

        [MapProperty(nameof(JobGrade.ReferenceId), nameof(PositionJobGradeDto.RefId))]
        public static partial PositionJobGradeDto ToJobGradeDto(this JobGrade grade);

        [MapProperty(nameof(CostCenter.ReferenceId), nameof(PositionCostCenterDto.RefId))]
        public static partial PositionCostCenterDto ToCostCenterDto(this CostCenter costCenter);

        private static DateTimeOffset GetCreatedOnUtc(Position entity) => entity.CreatedOnUtc;
        private static DateTimeOffset? GetLastModifiedOnUtc(Position entity) => entity.LastModifiedOnUtc;

        [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
        private static IReadOnlyList<PositionNodeDto> MapDirectReports(Position entity) =>
            entity.DirectReports
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.JobGrade?.RankOrder ?? 0)
                .ThenBy(p => p.Code)
                .Select(p => p.ToNodeDto())
                .ToList();
    }

    internal sealed record UpdatePositionRequest(
        Guid OrgUnitId,
        string JobRefId,
        string JobGradeRefId,
        string Code,
        string Title,
        string InternationalTitle,
        string? CostCenterRefId = null,
        string? ReportsToPositionRefId = null,
        string? DottedLinePositionRefId = null,
        int HeadcountCapacity = 1,
        bool IsPrimaryLeadership = false,
        DateTimeOffset? ValidFrom = null,
        DateTimeOffset? ValidTo = null);

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class PositionRequestMapper
    {
        internal static partial UpdatePositionCommand ToCommand(this UpdatePositionRequest request, string refId);
    }

    #endregion

    #region Specifications

    public sealed class SearchPositionsSpec : Specification<Position, PositionSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<Position, object>>> SortMappings =
            new Dictionary<string, Expression<Func<Position, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = p => p.Code,
                ["title"] = p => p.Title,
                ["internationalTitle"] = p => p.InternationalTitle,
                ["rankOrder"] = p => p.JobGrade!.RankOrder,
                ["isPrimaryLeadership"] = p => p.IsPrimaryLeadership,
                ["isActive"] = p => p.IsActive,
                ["createdOnUtc"] = p => p.CreatedOnUtc
            };

        public SearchPositionsSpec(SearchPositionsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
            {
                var leRef = request.LegalEntityRefId.Trim();
                Where(p => p.LegalEntity!.ReferenceId == leRef);
            }

            if (request.OrgUnitId.HasValue)
            {
                Where(p => p.OrgUnitId == request.OrgUnitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.JobRefId))
            {
                var jobRef = request.JobRefId.Trim();
                Where(p => p.Job!.ReferenceId == jobRef);
            }

            if (!string.IsNullOrWhiteSpace(request.JobGradeRefId))
            {
                var gradeRef = request.JobGradeRefId.Trim();
                Where(p => p.JobGrade!.ReferenceId == gradeRef);
            }

            if (!string.IsNullOrWhiteSpace(request.CostCenterRefId))
            {
                var ccRef = request.CostCenterRefId.Trim();
                Where(p => p.CostCenter != null && p.CostCenter.ReferenceId == ccRef);
            }

            if (!string.IsNullOrWhiteSpace(request.ReportsToPositionRefId))
            {
                var managerRef = request.ReportsToPositionRefId.Trim();
                Where(p => p.ReportsToPosition != null && p.ReportsToPosition.ReferenceId == managerRef);
            }

            if (request.IsPrimaryLeadership.HasValue)
            {
                Where(p => p.IsPrimaryLeadership == request.IsPrimaryLeadership.Value);
            }

            if (request.IsActive.HasValue)
            {
                Where(p => p.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                Where(p =>
                    EF.Functions.ILike(p.Code, $"%{keyword}%") ||
                    EF.Functions.ILike(p.Title, $"%{keyword}%") ||
                    EF.Functions.ILike(p.InternationalTitle, $"%{keyword}%"));
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () => OrderByDescending(p => p.JobGrade!.RankOrder),
                sortMappings: SortMappings);

            Select(p => new PositionSummaryDto(
                p.ReferenceId,
                p.LegalEntity!.ReferenceId,
                p.LegalEntity!.LegalName,
                p.OrgUnitId,
                p.Job!.Code,
                p.JobGrade!.Code,
                p.JobGrade!.RankOrder,
                p.CostCenter != null ? p.CostCenter.Code : null,
                p.ReportsToPosition != null ? p.ReportsToPosition.ReferenceId : null,
                p.ReportsToPosition != null ? p.ReportsToPosition.Code : null,
                p.Code,
                p.Title,
                p.InternationalTitle,
                p.HeadcountCapacity,
                p.IsPrimaryLeadership,
                p.IsActive,
                p.DirectReports.Count(r => !r.IsDeleted && r.IsActive)));
        }
    }

    public sealed class GetPositionByRefIdSpec : Specification<Position>
    {
        public GetPositionByRefIdSpec(string refId)
        {
            Where(p => p.ReferenceId == refId);
            Include(p => p.LegalEntity!);
            Include(p => p.Job!);
            Include(p => p.JobGrade!);
            Include(p => p.CostCenter!);
            Include("ReportsToPosition.JobGrade");
            Include("DottedLinePosition.JobGrade");
            Include("DirectReports.JobGrade");
        }
    }

    #endregion

    #region Validators

    public sealed class SearchPositionsQueryValidator : AbstractValidator<SearchPositionsQuery>
    {
        public SearchPositionsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
            RuleFor(x => x.JobRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.JobRefId));
            RuleFor(x => x.JobGradeRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.JobGradeRefId));
            RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
            RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
        }
    }

    public sealed class GetPositionLookupsQueryValidator : AbstractValidator<GetPositionLookupsQuery>
    {
        public GetPositionLookupsQueryValidator() =>
            RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
    }

    public sealed class GetPositionByRefIdQueryValidator : AbstractValidator<GetPositionByRefIdQuery>
    {
        public GetPositionByRefIdQueryValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class GetPositionReportingChainQueryValidator : AbstractValidator<GetPositionReportingChainQuery>
    {
        public GetPositionReportingChainQueryValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.MaxDepth).InclusiveBetween(1, 50);
        }
    }

    public sealed class GetPositionSubtreeQueryValidator : AbstractValidator<GetPositionSubtreeQuery>
    {
        public GetPositionSubtreeQueryValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.MaxDepth).InclusiveBetween(1, 50);
        }
    }

    public sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
    {
        public CreatePositionCommandValidator()
        {
            RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.OrgUnitId).NotEmpty();
            RuleFor(x => x.JobRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(PositionRules.MaxCodeLength)
                .Must(PositionRules.IsValidCode)
                .WithMessage($"Code must be 2-{PositionRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Title).NotEmpty().MaximumLength(PositionRules.MaxTitleLength);
            RuleFor(x => x.InternationalTitle).NotEmpty().MaximumLength(PositionRules.MaxTitleLength);
            RuleFor(x => x.HeadcountCapacity).GreaterThanOrEqualTo(PositionRules.MinHeadcountCapacity);

            RuleFor(x => x.JobGradeRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.JobGradeRefId));
            RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
            RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
            RuleFor(x => x.DottedLinePositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

            RuleFor(x => x.DottedLinePositionRefId)
                .NotEqual(x => x.ReportsToPositionRefId)
                .WithMessage("Dotted-line reporting position cannot be the same as the primary reporting position.")
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId) &&
                           !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

            RuleFor(x => x.ValidTo)
                .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
        }
    }

    public sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
    {
        public UpdatePositionCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.OrgUnitId).NotEmpty();
            RuleFor(x => x.JobRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.JobGradeRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(PositionRules.MaxCodeLength)
                .Must(PositionRules.IsValidCode)
                .WithMessage($"Code must be 2-{PositionRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Title).NotEmpty().MaximumLength(PositionRules.MaxTitleLength);
            RuleFor(x => x.InternationalTitle).NotEmpty().MaximumLength(PositionRules.MaxTitleLength);
            RuleFor(x => x.HeadcountCapacity).GreaterThanOrEqualTo(PositionRules.MinHeadcountCapacity);

            RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
            RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
            RuleFor(x => x.DottedLinePositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

            RuleFor(x => x.ReportsToPositionRefId)
                .NotEqual(x => x.RefId)
                .WithMessage("A position cannot report to itself.")
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));

            RuleFor(x => x.DottedLinePositionRefId)
                .NotEqual(x => x.RefId)
                .WithMessage("A position cannot have a dotted-line report to itself.")
                .When(x => !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

            RuleFor(x => x.DottedLinePositionRefId)
                .NotEqual(x => x.ReportsToPositionRefId)
                .WithMessage("Dotted-line reporting position cannot be the same as the primary reporting position.")
                .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId) &&
                           !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

            RuleFor(x => x.ValidTo)
                .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
        }
    }

    public sealed class ActivatePositionCommandValidator : AbstractValidator<ActivatePositionCommand>
    {
        public ActivatePositionCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeactivatePositionCommandValidator : AbstractValidator<DeactivatePositionCommand>
    {
        public DeactivatePositionCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
    {
        public DeletePositionCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    #endregion

    #region Query Handlers

    public sealed class SearchPositionsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchPositionsQuery, PagedResponse<PositionSummaryDto>>
    {
        public async ValueTask<PagedResponse<PositionSummaryDto>> Handle(
            SearchPositionsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await dbContext.PositionsV2
                .ApplySpecification(new SearchPositionsSpec(request))
                .ToPagedResponseAsync(request, cancellationToken);
        }
    }

    public sealed class GetPositionLookupsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetPositionLookupsQuery, IReadOnlyList<PositionLookupDto>>
    {
        public async ValueTask<IReadOnlyList<PositionLookupDto>> Handle(
            GetPositionLookupsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = dbContext.PositionsV2.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
            {
                var leRef = request.LegalEntityRefId.Trim();
                query = query.Where(p => p.LegalEntity!.ReferenceId == leRef);
            }

            if (request.OrgUnitId.HasValue)
            {
                query = query.Where(p => p.OrgUnitId == request.OrgUnitId.Value);
            }

            if (request.ActiveOnly)
            {
                query = query.Where(p => p.IsActive);
            }

            if (request.LeadershipOnly)
            {
                query = query.Where(p => p.IsPrimaryLeadership);
            }

            return await query
                .OrderByDescending(p => p.JobGrade!.RankOrder)
                .ThenBy(p => p.Code)
                .Select(p => new PositionLookupDto(
                    p.ReferenceId,
                    p.LegalEntity!.ReferenceId,
                    p.OrgUnitId,
                    p.Code,
                    p.Title,
                    p.InternationalTitle,
                    p.JobGrade!.Code,
                    p.JobGrade!.RankOrder,
                    p.IsPrimaryLeadership))
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class GetPositionByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetPositionByRefIdQuery, PositionDetailDto>
    {
        public async ValueTask<PositionDetailDto> Handle(
            GetPositionByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.PositionsV2
                             .ApplySpecification(new GetPositionByRefIdSpec(request.RefId))
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

    public sealed class GetPositionReportingChainQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetPositionReportingChainQuery, IReadOnlyList<PositionNodeDto>>
    {
        public async ValueTask<IReadOnlyList<PositionNodeDto>> Handle(
            GetPositionReportingChainQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var start = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.ReferenceId == request.RefId)
                .Select(p => new { p.LegalEntityId, p.ReportsToPositionId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            if (!start.ReportsToPositionId.HasValue)
            {
                return [];
            }

            // Single-query projection of positions in the same LegalEntity (O(1) DB round-trip)
            var lookup = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.LegalEntityId == start.LegalEntityId)
                .Select(p => new
                {
                    p.Id,
                    p.ReportsToPositionId,
                    Node = new PositionNodeDto(
                        p.ReferenceId,
                        p.Code,
                        p.Title,
                        p.InternationalTitle,
                        p.JobGrade!.Code,
                        p.JobGrade!.RankOrder,
                        p.IsPrimaryLeadership,
                        p.IsActive)
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var chain = new List<PositionNodeDto>();
            var visited = new HashSet<Guid>();
            var cursor = start.ReportsToPositionId;

            while (cursor.HasValue && chain.Count < request.MaxDepth && visited.Add(cursor.Value))
            {
                if (!lookup.TryGetValue(cursor.Value, out var current))
                {
                    break;
                }

                if (request.IncludeInactive || current.Node.IsActive)
                {
                    chain.Add(current.Node);
                }

                cursor = current.ReportsToPositionId;
            }

            return chain;
        }
    }

    public sealed class GetPositionSubtreeQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetPositionSubtreeQuery, IReadOnlyList<PositionNodeDto>>
    {
        public async ValueTask<IReadOnlyList<PositionNodeDto>> Handle(
            GetPositionSubtreeQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var root = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.ReferenceId == request.RefId)
                .Select(p => new { p.Id, p.LegalEntityId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            // Single-query projection of all positions in the same LegalEntity (O(1) DB round-trip)
            var allPositions = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.LegalEntityId == root.LegalEntityId && p.ReportsToPositionId != null)
                .Select(p => new
                {
                    p.Id,
                    ManagerId = p.ReportsToPositionId!.Value,
                    Node = new PositionNodeDto(
                        p.ReferenceId,
                        p.Code,
                        p.Title,
                        p.InternationalTitle,
                        p.JobGrade!.Code,
                        p.JobGrade!.RankOrder,
                        p.IsPrimaryLeadership,
                        p.IsActive)
                })
                .ToListAsync(cancellationToken);

            var childrenByManager = allPositions
                .GroupBy(x => x.ManagerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Node.JobGradeRankOrder)
                          .ThenBy(x => x.Node.Code)
                          .ToList());

            var results = new List<PositionNodeDto>();
            var visited = new HashSet<Guid> { root.Id };
            var queue = new Queue<(Guid PositionId, int Depth)>();
            queue.Enqueue((root.Id, 0));

            while (queue.Count > 0)
            {
                var (currentId, depth) = queue.Dequeue();
                if (depth >= request.MaxDepth || !childrenByManager.TryGetValue(currentId, out var children))
                {
                    continue;
                }

                foreach (var child in children)
                {
                    if (!visited.Add(child.Id))
                    {
                        continue;
                    }

                    if (request.IncludeInactive || child.Node.IsActive)
                    {
                        results.Add(child.Node);
                    }

                    queue.Enqueue((child.Id, depth + 1));
                }
            }

            return results;
        }
    }

    #endregion

    #region Command Handlers & Shared Reference Resolver

    internal static class PositionReferenceResolver
    {
        public static async Task<Guid?> ResolveCostCenterIdAsync(
            OrganizationDbContext dbContext,
            Guid legalEntityId,
            string? costCenterRefId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(costCenterRefId))
            {
                return null;
            }

            var cc = await dbContext.CostCenters
                .AsNoTracking()
                .Where(c => c.ReferenceId == costCenterRefId)
                .Select(c => new { c.Id, c.LegalEntityId, c.IsActive })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Cost center '{costCenterRefId}' was not found.");

            if (cc.LegalEntityId != legalEntityId)
            {
                throw new InvalidOperationException("Cost center must belong to the same legal entity.");
            }

            if (!cc.IsActive)
            {
                throw new InvalidOperationException($"Cost center '{costCenterRefId}' is inactive.");
            }

            return cc.Id;
        }

        /// <summary>
        /// Resolves both ReportsToPositionRefId and DottedLinePositionRefId in a single database query.
        /// </summary>
        public static async Task<(Guid? ReportsToId, Guid? DottedLineId)> ResolveReportingTargetsAsync(
            OrganizationDbContext dbContext,
            Guid legalEntityId,
            string? reportsToRefId,
            string? dottedLineRefId,
            bool requireActiveTarget,
            CancellationToken cancellationToken)
        {
            var hasReportsTo = !string.IsNullOrWhiteSpace(reportsToRefId);
            var hasDottedLine = !string.IsNullOrWhiteSpace(dottedLineRefId);

            if (!hasReportsTo && !hasDottedLine)
            {
                return (null, null);
            }

            var targetRefIds = new List<string>(2);
            if (hasReportsTo) targetRefIds.Add(reportsToRefId!.Trim());
            if (hasDottedLine) targetRefIds.Add(dottedLineRefId!.Trim());

            var targets = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => targetRefIds.Contains(p.ReferenceId))
                .Select(p => new { p.Id, p.ReferenceId, p.LegalEntityId, p.IsActive })
                .ToDictionaryAsync(p => p.ReferenceId, StringComparer.Ordinal, cancellationToken);

            Guid? resolvedReportsToId = null;
            if (hasReportsTo)
            {
                var key = reportsToRefId!.Trim();
                if (!targets.TryGetValue(key, out var reportsTo))
                {
                    throw new NotFoundException($"Reporting position '{key}' was not found.");
                }

                if (reportsTo.LegalEntityId != legalEntityId)
                {
                    throw new InvalidOperationException("Reporting position must belong to the same legal entity.");
                }

                if (requireActiveTarget && !reportsTo.IsActive)
                {
                    throw new InvalidOperationException($"Reporting position '{key}' is inactive.");
                }

                resolvedReportsToId = reportsTo.Id;
            }

            Guid? resolvedDottedLineId = null;
            if (hasDottedLine)
            {
                var key = dottedLineRefId!.Trim();
                if (!targets.TryGetValue(key, out var dottedLine))
                {
                    throw new NotFoundException($"Dotted-line position '{key}' was not found.");
                }

                if (dottedLine.LegalEntityId != legalEntityId)
                {
                    throw new InvalidOperationException("Dotted-line position must belong to the same legal entity.");
                }

                if (requireActiveTarget && !dottedLine.IsActive)
                {
                    throw new InvalidOperationException($"Dotted-line position '{key}' is inactive.");
                }

                resolvedDottedLineId = dottedLine.Id;
            }

            return (resolvedReportsToId, resolvedDottedLineId);
        }

        public static async Task EnsureNoReportingCycleAsync(
            OrganizationDbContext dbContext,
            Guid legalEntityId,
            Guid positionId,
            Guid proposedReportsToId,
            CancellationToken cancellationToken)
        {
            if (positionId == proposedReportsToId)
            {
                throw new InvalidOperationException("A position cannot report to itself.");
            }

            var reportingMap = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.LegalEntityId == legalEntityId && p.ReportsToPositionId != null)
                .Select(p => new { p.Id, ManagerId = p.ReportsToPositionId!.Value })
                .ToDictionaryAsync(x => x.Id, x => x.ManagerId, cancellationToken);

            var visited = new HashSet<Guid> { positionId };
            Guid? cursor = proposedReportsToId;

            while (cursor.HasValue)
            {
                if (!visited.Add(cursor.Value))
                {
                    throw new InvalidOperationException(
                        "Assigning this reporting position would create a circular reporting hierarchy.");
                }

                cursor = reportingMap.TryGetValue(cursor.Value, out var nextManagerId)
                    ? nextManagerId
                    : null;
            }
        }
    }

    public sealed class CreatePositionCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreatePositionCommand, string>
    {
        public async ValueTask<string> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = PositionRules.NormalizeAndValidateCode(request.Code);

            // Single DB round-trip: resolve LegalEntity + check duplicate Position code in that entity
            var leLookup = await dbContext.LegalEntities
                .AsNoTracking()
                .Where(le => le.ReferenceId == request.LegalEntityRefId)
                .Select(le => new
                {
                    le.Id,
                    CodeExists = dbContext.PositionsV2.Any(p => p.LegalEntityId == le.Id && p.Code == normalizedCode)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

            if (leLookup.CodeExists)
            {
                throw new ConflictException(
                    $"Position code '{normalizedCode}' already exists in legal entity '{request.LegalEntityRefId}'.");
            }

            var job = await dbContext.Jobs
                .AsNoTracking()
                .Where(j => j.ReferenceId == request.JobRefId)
                .Select(j => new { j.Id, j.DefaultJobGradeId, j.IsActive })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job '{request.JobRefId}' was not found.");

            if (!job.IsActive)
            {
                throw new InvalidOperationException($"Job '{request.JobRefId}' is inactive.");
            }

            var jobGradeId = job.DefaultJobGradeId;
            if (!string.IsNullOrWhiteSpace(request.JobGradeRefId))
            {
                var grade = await dbContext.JobGrades
                    .AsNoTracking()
                    .Where(g => g.ReferenceId == request.JobGradeRefId)
                    .Select(g => new { g.Id, g.IsActive })
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Job grade '{request.JobGradeRefId}' was not found.");

                if (!grade.IsActive)
                {
                    throw new InvalidOperationException($"Job grade '{request.JobGradeRefId}' is inactive.");
                }

                jobGradeId = grade.Id;
            }

            var costCenterId = await PositionReferenceResolver.ResolveCostCenterIdAsync(
                dbContext, leLookup.Id, request.CostCenterRefId, cancellationToken);

            var (reportsToPositionId, dottedLinePositionId) =
                await PositionReferenceResolver.ResolveReportingTargetsAsync(
                    dbContext,
                    leLookup.Id,
                    request.ReportsToPositionRefId,
                    request.DottedLinePositionRefId,
                    request.IsActive,
                    cancellationToken);

            var position = Position.Create(
                legalEntityId: leLookup.Id,
                orgUnitId: request.OrgUnitId,
                jobId: job.Id,
                jobGradeId: jobGradeId,
                code: normalizedCode,
                title: request.Title,
                internationalTitle: request.InternationalTitle,
                costCenterId: costCenterId,
                reportsToPositionId: reportsToPositionId,
                dottedLinePositionId: dottedLinePositionId,
                headcountCapacity: request.HeadcountCapacity,
                isPrimaryLeadership: request.IsPrimaryLeadership,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo,
                isActive: request.IsActive);

            await dbContext.PositionsV2.AddAsync(position, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return position.ReferenceId;
        }
    }

    public sealed class UpdatePositionCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdatePositionCommand, string>
    {
        public async ValueTask<string> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = PositionRules.NormalizeAndValidateCode(request.Code);

            // Single DB round-trip: load tracked Position + scalar checks for duplicate Code, Job, and JobGrade
            var target = await dbContext.PositionsV2
                .Where(p => p.ReferenceId == request.RefId)
                .Select(p => new
                {
                    Entity = p,
                    CodeExists = p.Code != normalizedCode &&
                                 dbContext.PositionsV2.Any(other =>
                                     other.LegalEntityId == p.LegalEntityId &&
                                     other.Code == normalizedCode &&
                                     other.Id != p.Id),
                    JobId = dbContext.Jobs
                        .Where(j => j.ReferenceId == request.JobRefId)
                        .Select(j => (Guid?)j.Id)
                        .FirstOrDefault(),
                    JobIsActive = dbContext.Jobs
                        .Where(j => j.ReferenceId == request.JobRefId)
                        .Select(j => (bool?)j.IsActive)
                        .FirstOrDefault(),
                    GradeId = dbContext.JobGrades
                        .Where(g => g.ReferenceId == request.JobGradeRefId)
                        .Select(g => (Guid?)g.Id)
                        .FirstOrDefault(),
                    GradeIsActive = dbContext.JobGrades
                        .Where(g => g.ReferenceId == request.JobGradeRefId)
                        .Select(g => (bool?)g.IsActive)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            if (target.CodeExists)
            {
                throw new ConflictException($"Position code '{normalizedCode}' already exists in this legal entity.");
            }

            if (!target.JobId.HasValue)
            {
                throw new NotFoundException($"Job '{request.JobRefId}' was not found.");
            }

            if (target.JobIsActive != true)
            {
                throw new InvalidOperationException($"Job '{request.JobRefId}' is inactive.");
            }

            if (!target.GradeId.HasValue)
            {
                throw new NotFoundException($"Job grade '{request.JobGradeRefId}' was not found.");
            }

            if (target.GradeIsActive != true)
            {
                throw new InvalidOperationException($"Job grade '{request.JobGradeRefId}' is inactive.");
            }

            var position = target.Entity;

            var costCenterId = await PositionReferenceResolver.ResolveCostCenterIdAsync(
                dbContext, position.LegalEntityId, request.CostCenterRefId, cancellationToken);

            var (reportsToPositionId, dottedLinePositionId) =
                await PositionReferenceResolver.ResolveReportingTargetsAsync(
                    dbContext,
                    position.LegalEntityId,
                    request.ReportsToPositionRefId,
                    request.DottedLinePositionRefId,
                    position.IsActive,
                    cancellationToken);

            if (reportsToPositionId.HasValue && reportsToPositionId != position.ReportsToPositionId)
            {
                await PositionReferenceResolver.EnsureNoReportingCycleAsync(
                    dbContext, position.LegalEntityId, position.Id, reportsToPositionId.Value, cancellationToken);
            }

            var changed = position.Update(
                orgUnitId: request.OrgUnitId,
                jobId: target.JobId.Value,
                jobGradeId: target.GradeId.Value,
                code: normalizedCode,
                title: request.Title,
                internationalTitle: request.InternationalTitle,
                costCenterId: costCenterId,
                reportsToPositionId: reportsToPositionId,
                dottedLinePositionId: dottedLinePositionId,
                headcountCapacity: request.HeadcountCapacity,
                isPrimaryLeadership: request.IsPrimaryLeadership,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo);

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return position.ReferenceId;
        }
    }

    public sealed class ActivatePositionCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ActivatePositionCommand, string>
    {
        public async ValueTask<string> Handle(ActivatePositionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var position = await dbContext.PositionsV2
                               .Include(p => p.ReportsToPosition)
                               .FirstOrDefaultAsync(p => p.ReferenceId == request.RefId, cancellationToken)
                           ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            if (!position.IsActive)
            {
                position.Activate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return position.ReferenceId;
        }
    }

    public sealed class DeactivatePositionCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<DeactivatePositionCommand, string>
    {
        public async ValueTask<string> Handle(DeactivatePositionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Single DB round-trip: load tracked Position + check active direct reports via SQL EXISTS
            var target = await dbContext.PositionsV2
                .Where(p => p.ReferenceId == request.RefId)
                .Select(p => new
                {
                    Entity = p,
                    HasActiveDirectReports = dbContext.PositionsV2.Any(r => r.ReportsToPositionId == p.Id && r.IsActive)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            if (target.HasActiveDirectReports)
            {
                throw new InvalidOperationException(
                    "Cannot deactivate a position while active direct report positions still exist.");
            }

            if (target.Entity.IsActive)
            {
                target.Entity.Deactivate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return target.Entity.ReferenceId;
        }
    }

    public sealed class DeletePositionCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
        : IRequestHandler<DeletePositionCommand, string>
    {
        public async ValueTask<string> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Single DB round-trip: load tracked Position + check direct report or active delegation references
            var target = await dbContext.PositionsV2
                .Where(p => p.ReferenceId == request.RefId)
                .Select(p => new
                {
                    Entity = p,
                    HasDirectReports = dbContext.PositionsV2.Any(r => r.ReportsToPositionId == p.Id),
                    HasActiveDelegations = dbContext.PositionDelegations.Any(d =>
                        !d.IsRevoked &&
                        (d.DelegatorPositionId == p.Id || d.DelegatePositionId == p.Id))
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

            if (target.HasDirectReports)
            {
                throw new InvalidOperationException(
                    "Cannot delete a position while direct report positions still exist.");
            }

            if (target.HasActiveDelegations)
            {
                throw new InvalidOperationException(
                    "Cannot delete a position while active position delegations reference it.");
            }

            target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return target.Entity.ReferenceId;
        }
    }

    #endregion

    #region Minimal API Endpoints

    public static class PositionEndpoints
    {
        [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out")]
        public static IEndpointRouteBuilder MapPositionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/positions")
                .WithTags("Positions")
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
            
            group.MapGet("/", async (
                    [AsParameters] SearchPositionsQuery query,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
                .WithName("SearchPositions")
                .WithSummary("Search positions")
                .WithDescription("Searches and paginates organizational positions with reporting hierarchy and job grade metadata.")
                .Produces<PagedResponse<PositionSummaryDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
            
            group.MapGet("/lookups", async (
                    [FromQuery, Description("Optional legal entity reference ID filter.")] string? legalEntityRefId = null,
                    [FromQuery, Description("Optional organization unit ID filter.")] Guid? orgUnitId = null,
                    [FromQuery, Description("Filter to active positions only (default: true).")] bool activeOnly = true,
                    [FromQuery, Description("Filter to primary leadership positions only (default: false).")] bool leadershipOnly = false,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(
                        new GetPositionLookupsQuery(legalEntityRefId, orgUnitId, activeOnly, leadershipOnly),
                        cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetPositionLookups")
                .WithSummary("List position lookups")
                .WithDescription("Retrieves a lightweight list of positions ordered by authority rank for reporting and leadership selectors.")
                .Produces<IReadOnlyList<PositionLookupDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
            
            group.MapGet("/{refId}", async (
                    [Description("Unique reference ID of the position.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new GetPositionByRefIdQuery(refId), cancellationToken));
                })
                .WithName("GetPosition")
                .WithSummary("Get position by reference ID")
                .Produces<PositionDetailDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);
            
            group.MapGet("/{refId}/reporting-chain", async (
                    [Description("Starting position reference ID.")] string refId,
                    [FromQuery, Description("Maximum upward reporting levels to traverse (1-50).")] int maxDepth = 15,
                    [FromQuery, Description("Include inactive positions in the traversed chain.")] bool includeInactive = false,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(
                        new GetPositionReportingChainQuery(refId, maxDepth, includeInactive),
                        cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetPositionReportingChain")
                .WithSummary("Get upward reporting chain")
                .WithDescription("Resolves the ordered chain of command upward from a position for document approval routing.")
                .Produces<IReadOnlyList<PositionNodeDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);
            
            group.MapGet("/{refId}/subtree", async (
                    [Description("Root manager position reference ID.")] string refId,
                    [FromQuery, Description("Maximum downward reporting levels to traverse (1-50).")] int maxDepth = 10,
                    [FromQuery, Description("Include inactive positions in the subtree.")] bool includeInactive = false,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(
                        new GetPositionSubtreeQuery(refId, maxDepth, includeInactive),
                        cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetPositionSubtree")
                .WithSummary("Get subordinate reporting subtree")
                .WithDescription("Resolves all subordinate positions reporting beneath a manager position in breadth-first order.")
                .Produces<IReadOnlyList<PositionNodeDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);
            
            group.MapPost("/", async (
                    [FromBody] CreatePositionCommand command,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var refId = await mediator.Send(command, cancellationToken);
                    return Results.CreatedAtRoute("GetPosition", new { refId }, refId);
                })
                .WithName("CreatePosition")
                .WithSummary("Create a position")
                .Accepts<CreatePositionCommand>("application/json")
                .Produces<string>(StatusCodes.Status201Created, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);
            
            group.MapPut("/{refId}", async (
                    [Description("Unique reference ID of the position.")] string refId,
                    [FromBody] UpdatePositionRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
                })
                .WithName("UpdatePosition")
                .WithSummary("Update a position")
                .Accepts<UpdatePositionRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);
            
            group.MapPatch("/{refId}/activate", async (
                    [Description("Unique reference ID of the position.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new ActivatePositionCommand(refId), cancellationToken));
                })
                .WithName("ActivatePosition")
                .WithSummary("Activate a position")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);
            
            group.MapPatch("/{refId}/deactivate", async (
                    [Description("Unique reference ID of the position.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeactivatePositionCommand(refId), cancellationToken));
                })
                .WithName("DeactivatePosition")
                .WithSummary("Deactivate a position")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);
            
            group.MapDelete("/{refId}", async (
                    [Description("Unique reference ID of the position.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeletePositionCommand(refId), cancellationToken));
                })
                .WithName("DeletePosition")
                .WithSummary("Delete a position")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }
    }

    #endregion
}