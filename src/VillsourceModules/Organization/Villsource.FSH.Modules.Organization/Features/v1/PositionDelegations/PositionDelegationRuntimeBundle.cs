using System.ComponentModel;
using System.Linq.Expressions;
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
using Thinktecture;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;
using Villsource.Tool.UniqueKey;

using Position = Villsource.FSH.Modules.Organization.Domain.v2.Positions.Position;

// =============================================================================
// 1. DOMAIN LAYER (SmartEnum, Rules, Aggregate Root, Domain Events)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations
{
    [SmartEnum<string>]
    public sealed partial class DelegationType
    {
        public static readonly DelegationType ActingFull = new(
            key: "ACTING_FULL",
            displayName: "Full Acting Authority",
            requiresDocumentScope: false);

        public static readonly DelegationType ApprovalOnly = new(
            key: "APPROVAL_ONLY",
            displayName: "All Document Approvals",
            requiresDocumentScope: false);

        public static readonly DelegationType SpecificDocuments = new(
            key: "SPECIFIC_DOCUMENTS",
            displayName: "Specific Document Types Only",
            requiresDocumentScope: true);

        public string DisplayName { get; }
        public bool RequiresDocumentScope { get; }

        public static string AllowedKeysDescription =>
            string.Join(", ", Items.Select(i => i.Key));

        public static DelegationType FromKey(string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            var normalized = key.Trim().ToUpperInvariant();

            if (!TryGet(normalized, out var result))
            {
                throw new ArgumentException(
                    $"Invalid delegation type '{key}'. Allowed values: {AllowedKeysDescription}.",
                    nameof(key));
            }

            return result;
        }

        public static bool IsValidKey(string? key) =>
            !string.IsNullOrWhiteSpace(key) && TryGet(key.Trim().ToUpperInvariant(), out _);
    }

    internal static class PositionDelegationRules
    {
        public const int MaxDelegationTypeLength = 50;
        public const int MaxDocumentTypeScopeLength = 500;
        public const int MaxReasonLength = 500;
        public const int MaxRevokedByLength = 100;

        public static (DateTimeOffset UtcFrom, DateTimeOffset UtcTo, string? NormalizedScope, string? NormalizedReason)
            NormalizeAndValidate(
                DelegationType delegationType,
                DateTimeOffset validFrom,
                DateTimeOffset validTo,
                string? documentTypeScope,
                string? reason)
        {
            ArgumentNullException.ThrowIfNull(delegationType);

            var utcFrom = validFrom.ToUniversalTime();
            var utcTo = validTo.ToUniversalTime();

            if (utcTo <= utcFrom)
            {
                throw new ArgumentException("ValidTo must be strictly later than ValidFrom.", nameof(validTo));
            }

            var normalizedScope = NormalizeDocumentScope(delegationType, documentTypeScope);
            var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            return (utcFrom, utcTo, normalizedScope, normalizedReason);
        }

        public static string? NormalizeDocumentScope(DelegationType delegationType, string? scope)
        {
            if (!delegationType.RequiresDocumentScope)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException(
                    $"DocumentTypeScope is required when DelegationType is '{delegationType.Key}'.",
                    nameof(scope));
            }

            var tokens = scope
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            if (tokens.Length == 0)
            {
                throw new ArgumentException("At least one document type code must be provided.", nameof(scope));
            }

            var joined = string.Join(',', tokens);
            if (joined.Length > MaxDocumentTypeScopeLength)
            {
                throw new ArgumentException(
                    $"Normalized DocumentTypeScope exceeds maximum length of {MaxDocumentTypeScopeLength} characters.",
                    nameof(scope));
            }

            return joined;
        }
    }

    public sealed partial class PositionDelegation : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
    {
        private PositionDelegation() { }

        public string ReferenceId { get; } = VillsourceId.Key;
        public Guid LegalEntityId { get; private set; }
        public LegalEntity? LegalEntity { get; private set; } = null!;

        public Guid DelegatorPositionId { get; private set; }
        public Villsource.FSH.Modules.Organization.Domain.v2.Positions.Position? DelegatorPosition { get; private set; } = null!;

        public Guid DelegatePositionId { get; private set; }
        public Villsource.FSH.Modules.Organization.Domain.v2.Positions.Position? DelegatePosition { get; private set; } = null!;

        public DelegationType DelegationType { get; private set; } = null!;
        public string? DocumentTypeScope { get; private set; }
        public string? Reason { get; private set; }

        public DateTimeOffset ValidFrom { get; private set; }
        public DateTimeOffset ValidTo { get; private set; }

        public bool IsRevoked { get; private set; }
        public DateTimeOffset? RevokedOnUtc { get; private set; }
        public string? RevokedBy { get; private set; }
        public string? RevocationReason { get; private set; }

        public static PositionDelegation Create(
            Guid legalEntityId,
            Guid delegatorPositionId,
            Guid delegatePositionId,
            DelegationType delegationType,
            DateTimeOffset validFrom,
            DateTimeOffset validTo,
            string? documentTypeScope = null,
            string? reason = null)
        {
            if (legalEntityId == Guid.Empty) throw new ArgumentException("LegalEntityId cannot be empty.", nameof(legalEntityId));
            if (delegatorPositionId == Guid.Empty) throw new ArgumentException("DelegatorPositionId cannot be empty.", nameof(delegatorPositionId));
            if (delegatePositionId == Guid.Empty) throw new ArgumentException("DelegatePositionId cannot be empty.", nameof(delegatePositionId));
            if (delegatorPositionId == delegatePositionId)
            {
                throw new InvalidOperationException("A position cannot delegate authority to itself.");
            }

            var (utcFrom, utcTo, normalizedScope, normalizedReason) =
                PositionDelegationRules.NormalizeAndValidate(
                    delegationType, validFrom, validTo, documentTypeScope, reason);

            var entity = new PositionDelegation
            {
                Id = Guid.CreateVersion7(),
                LegalEntityId = legalEntityId,
                DelegatorPositionId = delegatorPositionId,
                DelegatePositionId = delegatePositionId,
                DelegationType = delegationType,
                DocumentTypeScope = normalizedScope,
                Reason = normalizedReason,
                ValidFrom = utcFrom,
                ValidTo = utcTo,
                IsRevoked = false
            };

            entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDelegationCreatedDomainEvent(
                    PositionDelegationId: entity.Id,
                    LegalEntityId: entity.LegalEntityId,
                    DelegatorPositionId: entity.DelegatorPositionId,
                    DelegatePositionId: entity.DelegatePositionId,
                    DelegationType: entity.DelegationType.Key,
                    ValidFrom: entity.ValidFrom,
                    ValidTo: entity.ValidTo,
                    EventId: id,
                    OccurredOnUtc: ts)));

            return entity;
        }

        /// <summary>
        /// Updates mutable attributes. Returns <c>true</c> if any field changed;
        /// returns <c>false</c> on no-op updates to skip redundant DB writes and events.
        /// </summary>
        public bool Update(
            DelegationType delegationType,
            DateTimeOffset validFrom,
            DateTimeOffset validTo,
            string? documentTypeScope = null,
            string? reason = null)
        {
            EnsureModifiable();

            var (utcFrom, utcTo, normalizedScope, normalizedReason) =
                PositionDelegationRules.NormalizeAndValidate(
                    delegationType, validFrom, validTo, documentTypeScope, reason);

            if (DelegationType == delegationType &&
                ValidFrom == utcFrom &&
                ValidTo == utcTo &&
                string.Equals(DocumentTypeScope, normalizedScope, StringComparison.Ordinal) &&
                string.Equals(Reason, normalizedReason, StringComparison.Ordinal))
            {
                return false;
            }

            DelegationType = delegationType;
            DocumentTypeScope = normalizedScope;
            Reason = normalizedReason;
            ValidFrom = utcFrom;
            ValidTo = utcTo;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDelegationUpdatedDomainEvent(
                    PositionDelegationId: Id,
                    LegalEntityId: LegalEntityId,
                    DelegationType: DelegationType.Key,
                    ValidFrom: ValidFrom,
                    ValidTo: ValidTo,
                    EventId: id,
                    OccurredOnUtc: ts)));

            return true;
        }

        public void Revoke(string? revokedBy = null, string? revocationReason = null)
        {
            EnsureNotDeleted();
            if (IsRevoked) return;

            IsRevoked = true;
            RevokedOnUtc = DateTimeOffset.UtcNow;
            RevokedBy = string.IsNullOrWhiteSpace(revokedBy) ? null : revokedBy.Trim();
            RevocationReason = string.IsNullOrWhiteSpace(revocationReason) ? null : revocationReason.Trim();

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDelegationRevokedDomainEvent(
                    PositionDelegationId: Id,
                    LegalEntityId: LegalEntityId,
                    DelegatorPositionId: DelegatorPositionId,
                    DelegatePositionId: DelegatePositionId,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public void Delete(string? deletedBy = null)
        {
            if (IsDeleted) return;

            DeletedOnUtc = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
            IsDeleted = true;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new PositionDelegationDeletedDomainEvent(
                    PositionDelegationId: Id,
                    LegalEntityId: LegalEntityId,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public bool IsEffectiveAt(DateTimeOffset utcTimestamp, string? documentTypeCode = null)
        {
            var target = utcTimestamp.ToUniversalTime();
            if (IsDeleted || IsRevoked || target < ValidFrom || target > ValidTo)
            {
                return false;
            }

            if (!DelegationType.RequiresDocumentScope || string.IsNullOrWhiteSpace(documentTypeCode))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(DocumentTypeScope))
            {
                return false;
            }

            var requestedDoc = documentTypeCode.Trim().ToUpperInvariant();
            return DocumentTypeScope
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(requestedDoc, StringComparer.OrdinalIgnoreCase);
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException($"Position delegation '{Id}' has already been deleted.");
            }
        }

        private void EnsureModifiable()
        {
            EnsureNotDeleted();
            if (IsRevoked)
            {
                throw new InvalidOperationException($"Position delegation '{Id}' has been revoked and cannot be modified.");
            }
        }
    }
}

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events
{
    public sealed record PositionDelegationCreatedDomainEvent(
        Guid PositionDelegationId,
        Guid LegalEntityId,
        Guid DelegatorPositionId,
        Guid DelegatePositionId,
        string DelegationType,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionDelegationUpdatedDomainEvent(
        Guid PositionDelegationId,
        Guid LegalEntityId,
        string DelegationType,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionDelegationRevokedDomainEvent(
        Guid PositionDelegationId,
        Guid LegalEntityId,
        Guid DelegatorPositionId,
        Guid DelegatePositionId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record PositionDelegationDeletedDomainEvent(
        Guid PositionDelegationId,
        Guid LegalEntityId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}

// =============================================================================
// 2. PERSISTENCE LAYER (EF Core Configuration)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Data.Configurations
{
    public sealed class PositionDelegationConfiguration : IEntityTypeConfiguration<PositionDelegation>
    {
        public void Configure(EntityTypeBuilder<PositionDelegation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("PositionDelegations");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedNever();

            // Unique & Partial Active-Window Indexes for fast approval resolution
            builder.HasIndex(d => d.ReferenceId).IsUnique();
            builder.HasIndex(d => new { d.DelegatorPositionId, d.ValidFrom, d.ValidTo })
                .HasFilter("\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");
            builder.HasIndex(d => new { d.DelegatePositionId, d.ValidFrom, d.ValidTo })
                .HasFilter("\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");
            builder.HasIndex(d => new { d.LegalEntityId, d.IsRevoked, d.ValidFrom });

            // Column Constraints (Single Source of Truth from PositionDelegationRules)
            builder.Property(d => d.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(d => d.DocumentTypeScope).HasMaxLength(PositionDelegationRules.MaxDocumentTypeScopeLength);
            builder.Property(d => d.Reason).HasMaxLength(PositionDelegationRules.MaxReasonLength);
            builder.Property(d => d.RevokedBy).HasMaxLength(PositionDelegationRules.MaxRevokedByLength);
            builder.Property(d => d.RevocationReason).HasMaxLength(PositionDelegationRules.MaxReasonLength);

            builder.Property(d => d.DelegationType)
                .HasConversion(
                    v => v.Key,
                    v => DelegationType.Get(v))
                .HasMaxLength(PositionDelegationRules.MaxDelegationTypeLength)
                .IsRequired();

            // Relationships
            builder.HasOne(d => d.LegalEntity)
                .WithMany()
                .HasForeignKey(d => d.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.DelegatorPosition)
                .WithMany()
                .HasForeignKey(d => d.DelegatorPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.DelegatePosition)
                .WithMany()
                .HasForeignKey(d => d.DelegatePositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// =============================================================================
// 3. APPLICATION & API LAYER (Mappers, Specs, Validators, Handlers, Endpoints)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations
{
    #region Mapperly Mappers

    internal sealed record UpdatePositionDelegationRequest(
        string DelegationType,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo,
        string? DocumentTypeScope = null,
        string? Reason = null);

    internal sealed record RevokePositionDelegationRequest(string? RevocationReason = null);

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class PositionDelegationRequestMapper
    {
        internal static partial UpdatePositionDelegationCommand ToCommand(
            this UpdatePositionDelegationRequest request,
            string refId);
    }

    #endregion

    #region Specifications

    public sealed class SearchPositionDelegationsSpec : Specification<PositionDelegation, PositionDelegationSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<PositionDelegation, object>>> SortMappings =
            new Dictionary<string, Expression<Func<PositionDelegation, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["validFrom"] = d => d.ValidFrom,
                ["validTo"] = d => d.ValidTo,
                ["isRevoked"] = d => d.IsRevoked,
                ["createdOnUtc"] = d => d.CreatedOnUtc
            };

        public SearchPositionDelegationsSpec(SearchPositionDelegationsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
            {
                var leRef = request.LegalEntityRefId.Trim();
                Where(d => d.LegalEntity!.ReferenceId == leRef);
            }

            if (!string.IsNullOrWhiteSpace(request.DelegatorPositionRefId))
            {
                var delegatorRef = request.DelegatorPositionRefId.Trim();
                Where(d => d.DelegatorPosition!.ReferenceId == delegatorRef);
            }

            if (!string.IsNullOrWhiteSpace(request.DelegatePositionRefId))
            {
                var delegateRef = request.DelegatePositionRefId.Trim();
                Where(d => d.DelegatePosition!.ReferenceId == delegateRef);
            }

            if (request.IsRevoked.HasValue)
            {
                Where(d => d.IsRevoked == request.IsRevoked.Value);
            }

            if (request.EffectiveAtUtc.HasValue)
            {
                var utcTarget = request.EffectiveAtUtc.Value.ToUniversalTime();
                Where(d => !d.IsRevoked && d.ValidFrom <= utcTarget && d.ValidTo >= utcTarget);
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () => OrderByDescending(d => d.ValidFrom),
                sortMappings: SortMappings);

            Select(d => new PositionDelegationSummaryDto(
                d.ReferenceId,
                d.LegalEntity!.ReferenceId,
                d.DelegatorPosition!.ReferenceId,
                d.DelegatorPosition!.Code,
                d.DelegatorPosition!.Title,
                d.DelegatePosition!.ReferenceId,
                d.DelegatePosition!.Code,
                d.DelegatePosition!.Title,
                (string)d.DelegationType,
                d.DocumentTypeScope,
                d.ValidFrom,
                d.ValidTo,
                d.IsRevoked));
        }
    }

    public sealed class GetPositionDelegationByRefIdSpec : Specification<PositionDelegation>
    {
        public GetPositionDelegationByRefIdSpec(string refId)
        {
            Where(d => d.ReferenceId == refId);
            Include(d => d.LegalEntity!);
            Include("DelegatorPosition.JobGrade");
            Include("DelegatePosition.JobGrade");
        }
    }

    #endregion

    #region Validators

    public sealed class SearchPositionDelegationsQueryValidator : AbstractValidator<SearchPositionDelegationsQuery>
    {
        public SearchPositionDelegationsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
            RuleFor(x => x.DelegatorPositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.DelegatorPositionRefId));
            RuleFor(x => x.DelegatePositionRefId).MaximumLength(VillsourceId.KeySizes)
                .When(x => !string.IsNullOrWhiteSpace(x.DelegatePositionRefId));
        }
    }

    public sealed class GetPositionDelegationByRefIdQueryValidator : AbstractValidator<GetPositionDelegationByRefIdQuery>
    {
        public GetPositionDelegationByRefIdQueryValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    public sealed class ResolveEffectiveDelegationsQueryValidator : AbstractValidator<ResolveEffectiveDelegationsQuery>
    {
        public ResolveEffectiveDelegationsQueryValidator()
        {
            RuleFor(x => x.DelegatorPositionRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.DocumentTypeCode).MaximumLength(100);
        }
    }

    public sealed class CreatePositionDelegationCommandValidator : AbstractValidator<CreatePositionDelegationCommand>
    {
        public CreatePositionDelegationCommandValidator()
        {
            RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.DelegatorPositionRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.DelegatePositionRefId)
                .NotEmpty()
                .MaximumLength(VillsourceId.KeySizes)
                .NotEqual(x => x.DelegatorPositionRefId)
                .WithMessage("DelegatePositionRefId cannot be the same as DelegatorPositionRefId.");

            RuleFor(x => x.DelegationType)
                .NotEmpty()
                .Must(DelegationType.IsValidKey)
                .WithMessage($"DelegationType must be one of: {DelegationType.AllowedKeysDescription}.");

            RuleFor(x => x.DocumentTypeScope).MaximumLength(PositionDelegationRules.MaxDocumentTypeScopeLength);
            RuleFor(x => x.Reason).MaximumLength(PositionDelegationRules.MaxReasonLength);
            RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
        }
    }

    public sealed class UpdatePositionDelegationCommandValidator : AbstractValidator<UpdatePositionDelegationCommand>
    {
        public UpdatePositionDelegationCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.DelegationType)
                .NotEmpty()
                .Must(DelegationType.IsValidKey)
                .WithMessage($"DelegationType must be one of: {DelegationType.AllowedKeysDescription}.");

            RuleFor(x => x.DocumentTypeScope).MaximumLength(PositionDelegationRules.MaxDocumentTypeScopeLength);
            RuleFor(x => x.Reason).MaximumLength(PositionDelegationRules.MaxReasonLength);
            RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
        }
    }

    public sealed class RevokePositionDelegationCommandValidator : AbstractValidator<RevokePositionDelegationCommand>
    {
        public RevokePositionDelegationCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.RevocationReason).MaximumLength(PositionDelegationRules.MaxReasonLength);
        }
    }

    public sealed class DeletePositionDelegationCommandValidator : AbstractValidator<DeletePositionDelegationCommand>
    {
        public DeletePositionDelegationCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

    #endregion

    #region Query Handlers

    public sealed class SearchPositionDelegationsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchPositionDelegationsQuery, PagedResponse<PositionDelegationSummaryDto>>
    {
        public async ValueTask<PagedResponse<PositionDelegationSummaryDto>> Handle(
            SearchPositionDelegationsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await dbContext.PositionDelegations
                .ApplySpecification(new SearchPositionDelegationsSpec(request))
                .ToPagedResponseAsync(request, cancellationToken);
        }
    }

    public sealed class GetPositionDelegationByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetPositionDelegationByRefIdQuery, PositionDelegationDetailDto>
    {
        public async ValueTask<PositionDelegationDetailDto> Handle(
            GetPositionDelegationByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.PositionDelegations
                             .ApplySpecification(new GetPositionDelegationByRefIdSpec(request.RefId))
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

    public sealed class GetDelegationTypesQueryHandler
        : IRequestHandler<GetDelegationTypesQuery, IReadOnlyList<DelegationTypeDto>>
    {
        private static readonly IReadOnlyList<DelegationTypeDto> CachedItems =
            DelegationType.Items.Select(x => x.ToDto()).ToList();

        public ValueTask<IReadOnlyList<DelegationTypeDto>> Handle(
            GetDelegationTypesQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromResult(CachedItems);
        }
    }

    public sealed class ResolveEffectiveDelegationsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ResolveEffectiveDelegationsQuery, IReadOnlyList<EffectiveDelegationDto>>
    {
        public async ValueTask<IReadOnlyList<EffectiveDelegationDto>> Handle(
            ResolveEffectiveDelegationsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var targetUtc = (request.AtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

            var candidates = await dbContext.PositionDelegations
                .AsNoTracking()
                .Include("DelegatePosition.JobGrade")
                .Where(d => d.DelegatorPosition!.ReferenceId == request.DelegatorPositionRefId &&
                            !d.IsRevoked &&
                            d.ValidFrom <= targetUtc &&
                            d.ValidTo >= targetUtc &&
                            d.DelegatePosition!.IsActive)
                .OrderByDescending(d => d.ValidFrom)
                .ToListAsync(cancellationToken);

            return candidates
                .Where(d => d.IsEffectiveAt(targetUtc, request.DocumentTypeCode))
                .Select(d => new EffectiveDelegationDto(
                    d.ReferenceId,
                    d.DelegationType.Key,
                    d.DocumentTypeScope,
                    d.ValidFrom,
                    d.ValidTo,
                    d.DelegatePosition!.ToPositionDto()))
                .ToList();
        }
    }

    #endregion

    #region Command Handlers

    public sealed class CreatePositionDelegationCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreatePositionDelegationCommand, string>
    {
        public async ValueTask<string> Handle(
            CreatePositionDelegationCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var legalEntityId = await dbContext.LegalEntities
                .AsNoTracking()
                .Where(le => le.ReferenceId == request.LegalEntityRefId)
                .Select(le => (Guid?)le.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

            var positions = await dbContext.PositionsV2
                .AsNoTracking()
                .Where(p => p.ReferenceId == request.DelegatorPositionRefId || p.ReferenceId == request.DelegatePositionRefId)
                .Select(p => new { p.Id, p.ReferenceId, p.LegalEntityId, p.IsActive })
                .ToListAsync(cancellationToken);

            var delegator = positions.FirstOrDefault(p => p.ReferenceId == request.DelegatorPositionRefId)
                ?? throw new NotFoundException($"Delegator position '{request.DelegatorPositionRefId}' was not found.");

            var delegatePos = positions.FirstOrDefault(p => p.ReferenceId == request.DelegatePositionRefId)
                ?? throw new NotFoundException($"Delegate position '{request.DelegatePositionRefId}' was not found.");

            if (delegator.LegalEntityId != legalEntityId || delegatePos.LegalEntityId != legalEntityId)
            {
                throw new InvalidOperationException("Both positions must belong to the specified legal entity.");
            }

            if (!delegator.IsActive || !delegatePos.IsActive)
            {
                throw new InvalidOperationException("Both delegator and delegate positions must be active.");
            }

            var delegationType = DelegationType.FromKey(request.DelegationType);
            var utcFrom = request.ValidFrom.ToUniversalTime();
            var utcTo = request.ValidTo.ToUniversalTime();

            // Single DB round-trip: detect both same-type overlap AND reciprocal ping-pong delegation overlap
            var conflictCheck = await dbContext.PositionDelegations
                .AsNoTracking()
                .Where(d => !d.IsRevoked &&
                            d.ValidFrom < utcTo &&
                            d.ValidTo > utcFrom &&
                            ((d.DelegatorPositionId == delegator.Id && d.DelegationType == delegationType) ||
                             (d.DelegatorPositionId == delegatePos.Id && d.DelegatePositionId == delegator.Id)))
                .Select(d => new { d.DelegatorPositionId })
                .FirstOrDefaultAsync(cancellationToken);

            if (conflictCheck is not null)
            {
                if (conflictCheck.DelegatorPositionId == delegator.Id)
                {
                    throw new ConflictException(
                        "An active delegation of the same type already overlaps with the specified validity period.");
                }

                throw new InvalidOperationException(
                    "Creating this delegation would form a reciprocal delegation loop during the overlapping period.");
            }

            var entity = PositionDelegation.Create(
                legalEntityId: legalEntityId,
                delegatorPositionId: delegator.Id,
                delegatePositionId: delegatePos.Id,
                delegationType: delegationType,
                validFrom: utcFrom,
                validTo: utcTo,
                documentTypeScope: request.DocumentTypeScope,
                reason: request.Reason);

            await dbContext.PositionDelegations.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

    public sealed class UpdatePositionDelegationCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdatePositionDelegationCommand, string>
    {
        public async ValueTask<string> Handle(
            UpdatePositionDelegationCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var delegationType = DelegationType.FromKey(request.DelegationType);
            var utcFrom = request.ValidFrom.ToUniversalTime();
            var utcTo = request.ValidTo.ToUniversalTime();

            // Single DB round-trip: fetch tracked entity + check overlapping same-type delegation
            var target = await dbContext.PositionDelegations
                .Where(d => d.ReferenceId == request.RefId)
                .Select(d => new
                {
                    Entity = d,
                    HasOverlap = dbContext.PositionDelegations.Any(other =>
                        other.DelegatorPositionId == d.DelegatorPositionId &&
                        other.Id != d.Id &&
                        !other.IsRevoked &&
                        other.DelegationType == delegationType &&
                        other.ValidFrom < utcTo &&
                        other.ValidTo > utcFrom)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

            if (target.HasOverlap)
            {
                throw new ConflictException(
                    "An active delegation of the same type already overlaps with the specified validity period.");
            }

            var changed = target.Entity.Update(
                delegationType: delegationType,
                validFrom: utcFrom,
                validTo: utcTo,
                documentTypeScope: request.DocumentTypeScope,
                reason: request.Reason);

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return target.Entity.ReferenceId;
        }
    }

    public sealed class RevokePositionDelegationCommandHandler(
        OrganizationDbContext dbContext,
        ICurrentUser currentUser)
        : IRequestHandler<RevokePositionDelegationCommand, string>
    {
        public async ValueTask<string> Handle(
            RevokePositionDelegationCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.PositionDelegations
                             .FirstOrDefaultAsync(d => d.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

            if (!entity.IsRevoked)
            {
                entity.Revoke(
                    revokedBy: currentUser.GetUserId().ToString(),
                    revocationReason: request.RevocationReason);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

    public sealed class DeletePositionDelegationCommandHandler(
        OrganizationDbContext dbContext,
        ICurrentUser currentUser)
        : IRequestHandler<DeletePositionDelegationCommand, string>
    {
        public async ValueTask<string> Handle(
            DeletePositionDelegationCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.PositionDelegations
                             .FirstOrDefaultAsync(d => d.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

            entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

    #endregion

    #region Minimal API Endpoints

    public static class PositionDelegationEndpoints
    {
        public static IEndpointRouteBuilder MapPositionDelegationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/position-delegations")
                .WithTags("Position Delegations")
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/", async (
                    [AsParameters] SearchPositionDelegationsQuery query,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
                .WithName("SearchPositionDelegations")
                .WithSummary("Search position delegations")
                .WithDescription("Searches and paginates temporary acting and approval authority delegations.")
                .Produces<PagedResponse<PositionDelegationSummaryDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/types", async (
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new GetDelegationTypesQuery(), cancellationToken));
                })
                .WithName("GetDelegationTypes")
                .WithSummary("List delegation types")
                .Produces<IReadOnlyList<DelegationTypeDto>>(StatusCodes.Status200OK, "application/json");

            group.MapGet("/effective", async (
                    [FromQuery, Description("Target delegator position reference ID.")] string delegatorPositionRefId,
                    [FromQuery, Description("Optional document type code to evaluate scope matching (e.g. PR, PO).")] string? documentTypeCode = null,
                    [FromQuery, Description("Evaluation timestamp in UTC (defaults to current UTC time).")] DateTimeOffset? atUtc = null,
                    ISender mediator = default!,
                    CancellationToken cancellationToken = default) =>
                {
                    var result = await mediator.Send(
                        new ResolveEffectiveDelegationsQuery(delegatorPositionRefId, documentTypeCode, atUtc),
                        cancellationToken);
                    return Results.Ok(result);
                })
                .WithName("ResolveEffectiveDelegations")
                .WithSummary("Resolve effective delegations for a position")
                .WithDescription("Returns active acting/approval delegations effective at the given timestamp and document type.")
                .Produces<IReadOnlyList<EffectiveDelegationDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/{refId}", async (
                    [Description("Unique reference ID of the position delegation.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new GetPositionDelegationByRefIdQuery(refId), cancellationToken));
                })
                .WithName("GetPositionDelegation")
                .WithSummary("Get position delegation by reference ID")
                .Produces<PositionDelegationDetailDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                    [FromBody] CreatePositionDelegationCommand command,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var refId = await mediator.Send(command, cancellationToken);
                    return Results.CreatedAtRoute("GetPositionDelegation", new { refId }, refId);
                })
                .WithName("CreatePositionDelegation")
                .WithSummary("Create a position delegation")
                .Accepts<CreatePositionDelegationCommand>("application/json")
                .Produces<string>(StatusCodes.Status201Created, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPut("/{refId}", async (
                    [Description("Unique reference ID of the position delegation.")] string refId,
                    [FromBody] UpdatePositionDelegationRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
                })
                .WithName("UpdatePositionDelegation")
                .WithSummary("Update a position delegation")
                .Accepts<UpdatePositionDelegationRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPatch("/{refId}/revoke", async (
                    [Description("Unique reference ID of the position delegation.")] string refId,
                    [FromBody] RevokePositionDelegationRequest? request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RevokePositionDelegationCommand(refId, request?.RevocationReason);
                    return Results.Ok(await mediator.Send(command, cancellationToken));
                })
                .WithName("RevokePositionDelegation")
                .WithSummary("Revoke a position delegation")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete("/{refId}", async (
                    [Description("Unique reference ID of the position delegation.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new DeletePositionDelegationCommand(refId), cancellationToken));
                })
                .WithName("DeletePositionDelegation")
                .WithSummary("Delete a position delegation")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }
    }

    #endregion
}