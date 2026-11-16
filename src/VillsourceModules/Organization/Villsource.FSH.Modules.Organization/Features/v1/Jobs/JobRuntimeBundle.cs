using System.ComponentModel;
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
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.Jobs.Events;
using Villsource.Tool.UniqueKey;

// =============================================================================
// 1. DOMAIN LAYER (Rules, Aggregate Root, Domain Events)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Domain.Jobs
{
    internal static partial class JobRules
    {
        public const int MaxCodeLength = 50;
        public const int MaxNameLength = 200;
        public const int MaxJobFamilyLength = 100;
        public const int MaxDescriptionLength = 1000;

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
                    $"Job code '{normalized}' must be 2-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                    nameof(code));
            }

            return normalized;
        }

        public static (string Code, string Name, string InternationalName, string? JobFamily, string? Description)
            NormalizeAndValidate(
                string code,
                string name,
                string internationalName,
                Guid defaultJobGradeId,
                string? jobFamily,
                string? description)
        {
            if (defaultJobGradeId == Guid.Empty)
            {
                throw new ArgumentException("DefaultJobGradeId cannot be empty.", nameof(defaultJobGradeId));
            }

            var normalizedCode = NormalizeAndValidateCode(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

            var normalizedFamily = string.IsNullOrWhiteSpace(jobFamily) ? null : jobFamily.Trim();
            var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            return (normalizedCode, name.Trim(), internationalName.Trim(), normalizedFamily, normalizedDesc);
        }
    }

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
}

namespace Villsource.FSH.Modules.Organization.Domain.Jobs.Events
{
    public sealed record JobCreatedDomainEvent(
        Guid JobId,
        string Code,
        string Name,
        Guid DefaultJobGradeId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobUpdatedDomainEvent(
        Guid JobId,
        string Code,
        string Name,
        Guid DefaultJobGradeId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobActivatedDomainEvent(
        Guid JobId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobDeactivatedDomainEvent(
        Guid JobId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobDeletedDomainEvent(
        Guid JobId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}

// =============================================================================
// 2. PERSISTENCE LAYER (EF Core Configuration)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Data.Configurations
{
    public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Jobs");
            builder.HasKey(j => j.Id);
            builder.Property(j => j.Id).ValueGeneratedNever();

            // Unique & Lookup Indexes
            builder.HasIndex(j => j.ReferenceId).IsUnique();
            builder.HasIndex(j => j.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(j => new { j.IsActive, j.JobFamily, j.Code }).HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(j => j.DefaultJobGradeId);

            // Column Constraints (Single Source of Truth from JobRules)
            builder.Property(j => j.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(j => j.Code).HasMaxLength(JobRules.MaxCodeLength).IsRequired();
            builder.Property(j => j.Name).HasMaxLength(JobRules.MaxNameLength).IsRequired();
            builder.Property(j => j.InternationalName).HasMaxLength(JobRules.MaxNameLength).IsRequired();
            builder.Property(j => j.JobFamily).HasMaxLength(JobRules.MaxJobFamilyLength);
            builder.Property(j => j.Description).HasMaxLength(JobRules.MaxDescriptionLength);

            // Relationships
            builder.HasOne(j => j.DefaultJobGrade)
                .WithMany()
                .HasForeignKey(j => j.DefaultJobGradeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// =============================================================================
// 3. APPLICATION & API LAYER (Mappers, Specs, Validators, Handlers, Endpoints)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs
{
    #region Mapperly Mappers

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class JobMapper
    {
        [MapProperty(nameof(Job.ReferenceId), nameof(JobDetailDto.RefId))]
        [MapPropertyFromSource(nameof(JobDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
        [MapPropertyFromSource(nameof(JobDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
        public static partial JobDetailDto ToDetailDto(this Job entity);

        [MapProperty(nameof(JobGrade.ReferenceId), nameof(JobGradeRefDto.RefId))]
        public static partial JobGradeRefDto ToRefDto(this JobGrade grade);

        private static DateTimeOffset GetCreatedOnUtc(Job entity) => entity.CreatedOnUtc;
        private static DateTimeOffset? GetLastModifiedOnUtc(Job entity) => entity.LastModifiedOnUtc;
    }

    internal sealed record UpdateJobRequest(
        string Code,
        string Name,
        string InternationalName,
        string DefaultJobGradeRefId,
        string? JobFamily = null,
        string? Description = null);

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class JobRequestMapper
    {
        internal static partial UpdateJobCommand ToCommand(this UpdateJobRequest request, string refId);
    }

    #endregion

    #region Specifications

    public sealed class SearchJobsSpec : Specification<Job, JobSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<Job, object>>> SortMappings =
            new Dictionary<string, Expression<Func<Job, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = j => j.Code,
                ["name"] = j => j.Name,
                ["internationalName"] = j => j.InternationalName,
                ["jobFamily"] = j => j.JobFamily!,
                ["rankOrder"] = j => j.DefaultJobGrade!.RankOrder,
                ["isActive"] = j => j.IsActive,
                ["createdOnUtc"] = j => j.CreatedOnUtc
            };

        public SearchJobsSpec(SearchJobsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.JobFamily))
            {
                var family = request.JobFamily.Trim();
                Where(j => j.JobFamily != null && EF.Functions.ILike(j.JobFamily, family));
            }

            if (!string.IsNullOrWhiteSpace(request.DefaultJobGradeRefId))
            {
                var gradeRefId = request.DefaultJobGradeRefId.Trim();
                Where(j => j.DefaultJobGrade!.ReferenceId == gradeRefId);
            }

            if (request.IsActive.HasValue)
            {
                Where(j => j.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                Where(j =>
                    EF.Functions.ILike(j.Code, $"%{keyword}%") ||
                    EF.Functions.ILike(j.Name, $"%{keyword}%") ||
                    EF.Functions.ILike(j.InternationalName, $"%{keyword}%") ||
                    (j.JobFamily != null && EF.Functions.ILike(j.JobFamily, $"%{keyword}%")));
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () => OrderBy(j => j.Code),
                sortMappings: SortMappings);

            Select(j => new JobSummaryDto(
                j.ReferenceId,
                j.Code,
                j.Name,
                j.InternationalName,
                j.JobFamily,
                j.DefaultJobGrade!.ReferenceId,
                j.DefaultJobGrade!.Code,
                j.DefaultJobGrade!.RankOrder,
                j.IsActive));
        }
    }

    public sealed class GetJobByRefIdSpec : Specification<Job>
    {
        public GetJobByRefIdSpec(string refId)
        {
            Where(j => j.ReferenceId == refId);
            Include(j => j.DefaultJobGrade!);
        }
    }

    #endregion

    #region Validators

    public sealed class SearchJobsQueryValidator : AbstractValidator<SearchJobsQuery>
    {
        public SearchJobsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
            RuleFor(x => x.DefaultJobGradeRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.DefaultJobGradeRefId));
        }
    }

    public sealed class GetJobLookupsQueryValidator : AbstractValidator<GetJobLookupsQuery>
    {
        public GetJobLookupsQueryValidator() =>
            RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
    }

    public sealed class GetJobByRefIdQueryValidator : AbstractValidator<GetJobByRefIdQuery>
    {
        public GetJobByRefIdQueryValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
    {
        public CreateJobCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobRules.MaxCodeLength)
                .Must(JobRules.IsValidCode)
                .WithMessage($"Code must be 2-{JobRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobRules.MaxNameLength);
            RuleFor(x => x.DefaultJobGradeRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
            RuleFor(x => x.Description).MaximumLength(JobRules.MaxDescriptionLength);
        }
    }

    public sealed class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
    {
        public UpdateJobCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobRules.MaxCodeLength)
                .Must(JobRules.IsValidCode)
                .WithMessage($"Code must be 2-{JobRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobRules.MaxNameLength);
            RuleFor(x => x.DefaultJobGradeRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
            RuleFor(x => x.Description).MaximumLength(JobRules.MaxDescriptionLength);
        }
    }

    public sealed class ActivateJobCommandValidator : AbstractValidator<ActivateJobCommand>
    {
        public ActivateJobCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeactivateJobCommandValidator : AbstractValidator<DeactivateJobCommand>
    {
        public DeactivateJobCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeleteJobCommandValidator : AbstractValidator<DeleteJobCommand>
    {
        public DeleteJobCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    #endregion

    #region Query Handlers

    public sealed class SearchJobsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchJobsQuery, PagedResponse<JobSummaryDto>>
    {
        public async ValueTask<PagedResponse<JobSummaryDto>> Handle(
            SearchJobsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await dbContext.Jobs
                .ApplySpecification(new SearchJobsSpec(request))
                .ToPagedResponseAsync(request, cancellationToken);
        }
    }

    public sealed class GetJobLookupsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobLookupsQuery, IReadOnlyList<JobLookupDto>>
    {
        public async ValueTask<IReadOnlyList<JobLookupDto>> Handle(
            GetJobLookupsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = dbContext.Jobs.AsNoTracking();

            if (request.ActiveOnly)
            {
                query = query.Where(j => j.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.JobFamily))
            {
                var family = request.JobFamily.Trim();
                query = query.Where(j => j.JobFamily != null && EF.Functions.ILike(j.JobFamily, family));
            }

            return await query
                .OrderBy(j => j.Code)
                .Select(j => new JobLookupDto(
                    j.ReferenceId,
                    j.Code,
                    j.Name,
                    j.InternationalName,
                    j.JobFamily,
                    j.DefaultJobGrade!.ReferenceId,
                    j.DefaultJobGrade!.Code,
                    j.DefaultJobGrade!.RankOrder))
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class GetJobByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobByRefIdQuery, JobDetailDto>
    {
        public async ValueTask<JobDetailDto> Handle(
            GetJobByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.Jobs
                             .ApplySpecification(new GetJobByRefIdSpec(request.RefId))
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

    #endregion

    #region Command Handlers

    public sealed class CreateJobCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreateJobCommand, string>
    {
        public async ValueTask<string> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = JobRules.NormalizeAndValidateCode(request.Code);

            // Single DB round-trip: resolve DefaultJobGrade + check duplicate Code simultaneously
            var lookup = await dbContext.JobGrades
                .AsNoTracking()
                .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                .Select(g => new
                {
                    g.Id,
                    g.IsActive,
                    CodeExists = dbContext.Jobs.Any(j => j.Code == normalizedCode)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job grade '{request.DefaultJobGradeRefId}' was not found.");

            if (!lookup.IsActive)
            {
                throw new InvalidOperationException($"Job grade '{request.DefaultJobGradeRefId}' is inactive.");
            }

            if (lookup.CodeExists)
            {
                throw new ConflictException($"Job code '{normalizedCode}' already exists.");
            }

            var entity = Job.Create(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                defaultJobGradeId: lookup.Id,
                jobFamily: request.JobFamily,
                description: request.Description,
                isActive: request.IsActive);

            await dbContext.Jobs.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

    public sealed class UpdateJobCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdateJobCommand, string>
    {
        public async ValueTask<string> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = JobRules.NormalizeAndValidateCode(request.Code);

            // Single DB round-trip: load tracked Job + scalar subqueries for duplicate Code and target JobGrade
            var target = await dbContext.Jobs
                .Where(j => j.ReferenceId == request.RefId)
                .Select(j => new
                {
                    Entity = j,
                    CodeExists = j.Code != normalizedCode &&
                                 dbContext.Jobs.Any(other => other.Code == normalizedCode && other.Id != j.Id),
                    GradeId = dbContext.JobGrades
                        .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                        .Select(g => (Guid?)g.Id)
                        .FirstOrDefault(),
                    GradeIsActive = dbContext.JobGrades
                        .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                        .Select(g => (bool?)g.IsActive)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

            if (target.CodeExists)
            {
                throw new ConflictException($"Job code '{normalizedCode}' already exists.");
            }

            if (!target.GradeId.HasValue)
            {
                throw new NotFoundException($"Job grade '{request.DefaultJobGradeRefId}' was not found.");
            }

            if (target.GradeIsActive != true)
            {
                throw new InvalidOperationException($"Job grade '{request.DefaultJobGradeRefId}' is inactive.");
            }

            var changed = target.Entity.Update(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                defaultJobGradeId: target.GradeId.Value,
                jobFamily: request.JobFamily,
                description: request.Description);

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return target.Entity.ReferenceId;
        }
    }

    public sealed class ActivateJobCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ActivateJobCommand, string>
    {
        public async ValueTask<string> Handle(ActivateJobCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.Jobs
                             .FirstOrDefaultAsync(j => j.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

            if (!entity.IsActive)
            {
                entity.Activate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class DeactivateJobCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<DeactivateJobCommand, string>
    {
        public async ValueTask<string> Handle(DeactivateJobCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.Jobs
                             .FirstOrDefaultAsync(j => j.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

            if (entity.IsActive)
            {
                entity.Deactivate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class DeleteJobCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
        : IRequestHandler<DeleteJobCommand, string>
    {
        public async ValueTask<string> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Single DB round-trip: fetch tracked Job + check active Position references
            var target = await dbContext.Jobs
                .Where(j => j.ReferenceId == request.RefId)
                .Select(j => new
                {
                    Entity = j,
                    HasPositions = dbContext.PositionsV2.Any(p => p.JobId == j.Id)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

            if (target.HasPositions)
            {
                throw new InvalidOperationException(
                    $"Cannot delete job '{request.RefId}' while positions are assigned to it.");
            }

            target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return target.Entity.ReferenceId;
        }
    }

    #endregion

    #region Minimal API Endpoints

    public static class JobEndpoints
    {
        public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/jobs")
                .WithTags("Jobs")
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/", async (
                    [AsParameters] SearchJobsQuery query,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
                .WithName("SearchJobs")
                .WithSummary("Search jobs")
                .WithDescription("Searches and paginates standardized job templates.")
                .Produces<PagedResponse<JobSummaryDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/lookups", async (
                    [FromQuery, Description("Filter to active jobs only (default: true).")] bool activeOnly = true,
                    [FromQuery, Description("Optional job family filter.")] string? jobFamily = null,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(new GetJobLookupsQuery(activeOnly, jobFamily), cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetJobLookups")
                .WithSummary("List job lookups")
                .WithDescription("Retrieves a lightweight list of jobs with default grade metadata for dropdown selectors.")
                .Produces<IReadOnlyList<JobLookupDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/{refId}", async (
                    [Description("Unique reference ID of the job.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new GetJobByRefIdQuery(refId), cancellationToken));
                })
                .WithName("GetJob")
                .WithSummary("Get job by reference ID")
                .Produces<JobDetailDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                    [FromBody] CreateJobCommand command,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var refId = await mediator.Send(command, cancellationToken);
                    return Results.CreatedAtRoute("GetJob", new { refId }, refId);
                })
                .WithName("CreateJob")
                .WithSummary("Create a job")
                .Accepts<CreateJobCommand>("application/json")
                .Produces<string>(StatusCodes.Status201Created, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPut("/{refId}", async (
                    [Description("Unique reference ID of the job.")] string refId,
                    [FromBody] UpdateJobRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
                })
                .WithName("UpdateJob")
                .WithSummary("Update a job")
                .Accepts<UpdateJobRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPatch("/{refId}/activate", async (
                    [Description("Unique reference ID of the job.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new ActivateJobCommand(refId), cancellationToken));
                })
                .WithName("ActivateJob")
                .WithSummary("Activate a job")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPatch("/{refId}/deactivate", async (
                    [Description("Unique reference ID of the job.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeactivateJobCommand(refId), cancellationToken));
                })
                .WithName("DeactivateJob")
                .WithSummary("Deactivate a job")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete("/{refId}", async (
                    [Description("Unique reference ID of the job.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeleteJobCommand(refId), cancellationToken));
                })
                .WithName("DeleteJob")
                .WithSummary("Delete a job")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }
    }

    #endregion
}