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
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.JobGrades.Events;
using Villsource.Tool.UniqueKey;

// =============================================================================
// 1. DOMAIN LAYER (Rules, Aggregate Root, Domain Events)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Domain.JobGrades
{
    internal static partial class JobGradeRules
    {
        public const int MaxCodeLength = 20;
        public const int MaxNameLength = 150;
        public const int MaxDescriptionLength = 500;
        public const int MinRankOrder = 1;

        [GeneratedRegex(@"^[A-Z0-9\-_]{1,20}$", RegexOptions.CultureInvariant)]
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
                    $"Job grade code '{normalized}' must be 1-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                    nameof(code));
            }

            return normalized;
        }

        public static (string Code, string Name, string InternationalName, string? Description) NormalizeAndValidate(
            string code,
            string name,
            string internationalName,
            int rankOrder,
            string? description)
        {
            var normalizedCode = NormalizeAndValidateCode(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

            if (rankOrder < MinRankOrder)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rankOrder),
                    $"RankOrder must be greater than or equal to {MinRankOrder}.");
            }

            var normalizedDescription = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();

            return (normalizedCode, name.Trim(), internationalName.Trim(), normalizedDescription);
        }
    }

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

namespace Villsource.FSH.Modules.Organization.Domain.JobGrades.Events
{
    public sealed record JobGradeCreatedDomainEvent(
        Guid JobGradeId,
        string Code,
        string Name,
        int RankOrder,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobGradeUpdatedDomainEvent(
        Guid JobGradeId,
        string Code,
        string Name,
        int RankOrder,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobGradeActivatedDomainEvent(
        Guid JobGradeId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobGradeDeactivatedDomainEvent(
        Guid JobGradeId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record JobGradeDeletedDomainEvent(
        Guid JobGradeId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}

// =============================================================================
// 2. PERSISTENCE LAYER (EF Core Configuration)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Data.Configurations
{
    public sealed class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
    {
        public void Configure(EntityTypeBuilder<JobGrade> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("JobGrades");
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id).ValueGeneratedNever();

            // Unique & Lookup Indexes
            builder.HasIndex(g => g.ReferenceId).IsUnique();
            builder.HasIndex(g => g.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(g => new { g.IsActive, g.RankOrder, g.Code }).HasFilter("\"IsDeleted\" = FALSE");

            // Column Constraints (Single Source of Truth from JobGradeRules)
            builder.Property(g => g.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(g => g.Code).HasMaxLength(JobGradeRules.MaxCodeLength).IsRequired();
            builder.Property(g => g.Name).HasMaxLength(JobGradeRules.MaxNameLength).IsRequired();
            builder.Property(g => g.InternationalName).HasMaxLength(JobGradeRules.MaxNameLength).IsRequired();
            builder.Property(g => g.Description).HasMaxLength(JobGradeRules.MaxDescriptionLength);
        }
    }
}

// =============================================================================
// 3. APPLICATION & API LAYER (Mappers, Specs, Validators, Handlers, Endpoints)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades
{
    #region Mapperly Mappers

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class JobGradeMapper
    {
        [MapProperty(nameof(JobGrade.ReferenceId), nameof(JobGradeDetailDto.RefId))]
        [MapPropertyFromSource(nameof(JobGradeDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
        [MapPropertyFromSource(nameof(JobGradeDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
        public static partial JobGradeDetailDto ToDetailDto(this JobGrade entity);

        private static DateTimeOffset GetCreatedOnUtc(JobGrade entity) => entity.CreatedOnUtc;
        private static DateTimeOffset? GetLastModifiedOnUtc(JobGrade entity) => entity.LastModifiedOnUtc;
    }

    internal sealed record UpdateJobGradeRequest(
        string Code,
        string Name,
        string InternationalName,
        int RankOrder,
        string? Description = null);

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class JobGradeRequestMapper
    {
        internal static partial UpdateJobGradeCommand ToCommand(this UpdateJobGradeRequest request, string refId);
    }

    #endregion

    #region Specifications

    public sealed class SearchJobGradesSpec : Specification<JobGrade, JobGradeSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<JobGrade, object>>> SortMappings =
            new Dictionary<string, Expression<Func<JobGrade, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = g => g.Code,
                ["name"] = g => g.Name,
                ["internationalName"] = g => g.InternationalName,
                ["rankOrder"] = g => g.RankOrder,
                ["isActive"] = g => g.IsActive,
                ["createdOnUtc"] = g => g.CreatedOnUtc
            };

        public SearchJobGradesSpec(SearchJobGradesQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.IsActive.HasValue)
            {
                Where(g => g.IsActive == request.IsActive.Value);
            }

            if (request.MinRankOrder.HasValue)
            {
                Where(g => g.RankOrder >= request.MinRankOrder.Value);
            }

            if (request.MaxRankOrder.HasValue)
            {
                Where(g => g.RankOrder <= request.MaxRankOrder.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                Where(g =>
                    EF.Functions.ILike(g.Code, $"%{keyword}%") ||
                    EF.Functions.ILike(g.Name, $"%{keyword}%") ||
                    EF.Functions.ILike(g.InternationalName, $"%{keyword}%"));
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () => OrderBy(g => g.RankOrder),
                sortMappings: SortMappings);

            Select(g => new JobGradeSummaryDto(
                g.ReferenceId,
                g.Code,
                g.Name,
                g.InternationalName,
                g.RankOrder,
                g.Description,
                g.IsActive));
        }
    }

    #endregion

    #region Validators

    public sealed class SearchJobGradesQueryValidator : AbstractValidator<SearchJobGradesQuery>
    {
        public SearchJobGradesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.MinRankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder).When(x => x.MinRankOrder.HasValue);
            RuleFor(x => x.MaxRankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder).When(x => x.MaxRankOrder.HasValue);
            RuleFor(x => x.MaxRankOrder)
                .GreaterThanOrEqualTo(x => x.MinRankOrder!.Value)
                .When(x => x.MinRankOrder.HasValue && x.MaxRankOrder.HasValue);
        }
    }

    public sealed class GetJobGradeByRefIdQueryValidator : AbstractValidator<GetJobGradeByRefIdQuery>
    {
        public GetJobGradeByRefIdQueryValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class CreateJobGradeCommandValidator : AbstractValidator<CreateJobGradeCommand>
    {
        public CreateJobGradeCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobGradeRules.MaxCodeLength)
                .Must(JobGradeRules.IsValidCode)
                .WithMessage($"Code must be 1-{JobGradeRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.RankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder);
            RuleFor(x => x.Description).MaximumLength(JobGradeRules.MaxDescriptionLength);
        }
    }

    public sealed class UpdateJobGradeCommandValidator : AbstractValidator<UpdateJobGradeCommand>
    {
        public UpdateJobGradeCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobGradeRules.MaxCodeLength)
                .Must(JobGradeRules.IsValidCode)
                .WithMessage($"Code must be 1-{JobGradeRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.RankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder);
            RuleFor(x => x.Description).MaximumLength(JobGradeRules.MaxDescriptionLength);
        }
    }

    public sealed class ActivateJobGradeCommandValidator : AbstractValidator<ActivateJobGradeCommand>
    {
        public ActivateJobGradeCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeactivateJobGradeCommandValidator : AbstractValidator<DeactivateJobGradeCommand>
    {
        public DeactivateJobGradeCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class DeleteJobGradeCommandValidator : AbstractValidator<DeleteJobGradeCommand>
    {
        public DeleteJobGradeCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    #endregion

    #region Query Handlers

    public sealed class SearchJobGradesQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchJobGradesQuery, PagedResponse<JobGradeSummaryDto>>
    {
        public async ValueTask<PagedResponse<JobGradeSummaryDto>> Handle(
            SearchJobGradesQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await dbContext.JobGrades
                .ApplySpecification(new SearchJobGradesSpec(request))
                .ToPagedResponseAsync(request, cancellationToken);
        }
    }

    public sealed class GetJobGradeLookupsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobGradeLookupsQuery, IReadOnlyList<JobGradeLookupDto>>
    {
        public async ValueTask<IReadOnlyList<JobGradeLookupDto>> Handle(
            GetJobGradeLookupsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = dbContext.JobGrades.AsNoTracking();

            if (request.ActiveOnly)
            {
                query = query.Where(g => g.IsActive);
            }

            return await query
                .OrderBy(g => g.RankOrder)
                .ThenBy(g => g.Code)
                .Select(g => new JobGradeLookupDto(
                    g.ReferenceId,
                    g.Code,
                    g.Name,
                    g.InternationalName,
                    g.RankOrder))
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class GetJobGradeByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobGradeByRefIdQuery, JobGradeDetailDto>
    {
        public async ValueTask<JobGradeDetailDto> Handle(
            GetJobGradeByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .AsNoTracking()
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

    #endregion

    #region Command Handlers

    public sealed class CreateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(
            CreateJobGradeCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = JobGradeRules.NormalizeAndValidateCode(request.Code);

            if (await dbContext.JobGrades.AnyAsync(g => g.Code == normalizedCode, cancellationToken))
            {
                throw new ConflictException($"Job grade code '{normalizedCode}' already exists.");
            }

            var entity = JobGrade.Create(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                rankOrder: request.RankOrder,
                description: request.Description,
                isActive: request.IsActive);

            await dbContext.JobGrades.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

    public sealed class UpdateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(
            UpdateJobGradeCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            var normalizedCode = JobGradeRules.NormalizeAndValidateCode(request.Code);

            if (!string.Equals(entity.Code, normalizedCode, StringComparison.Ordinal) &&
                await dbContext.JobGrades.AnyAsync(g => g.Code == normalizedCode && g.Id != entity.Id, cancellationToken))
            {
                throw new ConflictException($"Job grade code '{normalizedCode}' already exists.");
            }

            var changed = entity.Update(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                rankOrder: request.RankOrder,
                description: request.Description);

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class ActivateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ActivateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(ActivateJobGradeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            if (!entity.IsActive)
            {
                entity.Activate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class DeactivateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<DeactivateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(DeactivateJobGradeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            if (entity.IsActive)
            {
                entity.Deactivate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class DeleteJobGradeCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
        : IRequestHandler<DeleteJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(DeleteJobGradeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Single DB round-trip to fetch tracked entity + check active references
            var target = await dbContext.JobGrades
                .Where(g => g.ReferenceId == request.RefId)
                .Select(g => new
                {
                    Entity = g,
                    HasJobs = dbContext.Jobs.Any(j => j.DefaultJobGradeId == g.Id),
                    HasPositions = dbContext.PositionsV2.Any(p => p.JobGradeId == g.Id)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            if (target.HasJobs || target.HasPositions)
            {
                throw new InvalidOperationException(
                    $"Cannot delete job grade '{request.RefId}' while it is referenced by existing jobs or positions.");
            }

            target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return target.Entity.ReferenceId;
        }
    }

    #endregion

    #region Minimal API Endpoints

    public static class JobGradeEndpoints
    {
        public static IEndpointRouteBuilder MapJobGradeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/job-grades")
                .WithTags("Job Grades")
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/", async (
                    [AsParameters] SearchJobGradesQuery query,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(query, cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("SearchJobGrades")
                .WithSummary("Search job grades")
                .WithDescription("Searches and paginates job grades ordered by authority rank.")
                .Produces<PagedResponse<JobGradeSummaryDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/lookups", async (
                    [FromQuery, Description("Filter to active job grades only (default: true).")] bool activeOnly = true,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(new GetJobGradeLookupsQuery(activeOnly), cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetJobGradeLookups")
                .WithSummary("List job grade lookups")
                .WithDescription("Retrieves a lightweight list of job grades ordered by rank for dropdowns and approval matrix configuration.")
                .Produces<IReadOnlyList<JobGradeLookupDto>>(StatusCodes.Status200OK, "application/json");

            group.MapGet("/{refId}", async (
                    [Description("Unique reference ID of the job grade.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetJobGradeByRefIdQuery(refId), cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("GetJobGrade")
                .WithSummary("Get job grade by reference ID")
                .WithDescription("Retrieves job grade details and authority rank.")
                .Produces<JobGradeDetailDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                    [FromBody] CreateJobGradeCommand command,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var refId = await mediator.Send(command, cancellationToken);
                    return Results.CreatedAtRoute("GetJobGrade", new { refId }, refId);
                })
                .WithName("CreateJobGrade")
                .WithSummary("Create a job grade")
                .WithDescription("Creates a new job grade with seniority rank order.")
                .Accepts<CreateJobGradeCommand>("application/json")
                .Produces<string>(StatusCodes.Status201Created, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPut("/{refId}", async (
                    [Description("Unique reference ID of the job grade.")] string refId,
                    [FromBody] UpdateJobGradeRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var updatedRefId = await mediator.Send(request.ToCommand(refId), cancellationToken);
                    return Results.Ok(updatedRefId);
                })
                .WithName("UpdateJobGrade")
                .WithSummary("Update a job grade")
                .WithDescription("Updates an existing job grade.")
                .Accepts<UpdateJobGradeRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPatch("/{refId}/activate", async (
                    [Description("Unique reference ID of the job grade.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new ActivateJobGradeCommand(refId), cancellationToken));
                })
                .WithName("ActivateJobGrade")
                .WithSummary("Activate a job grade")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPatch("/{refId}/deactivate", async (
                    [Description("Unique reference ID of the job grade.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeactivateJobGradeCommand(refId), cancellationToken));
                })
                .WithName("DeactivateJobGrade")
                .WithSummary("Deactivate a job grade")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete("/{refId}", async (
                    [Description("Unique reference ID of the job grade.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeleteJobGradeCommand(refId), cancellationToken));
                })
                .WithName("DeleteJobGrade")
                .WithSummary("Delete a job grade")
                .WithDescription("Soft-deletes a job grade if no jobs or positions reference it.")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }
    }

    #endregion
}