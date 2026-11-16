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
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;
using Villsource.FSH.Modules.Organization.Domain.v2.Positions;
using Villsource.Tool.UniqueKey;

#region 1. Domain & Domain Events

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events
{
    public sealed record OrganizationUnitCreatedDomainEvent(
        Guid OrganizationUnitId,
        string Code,
        string Name,
        string InternationalName,
        string Type,
        Guid? LegalEntityId,
        Guid? ParentId,
        Guid? ManagerPositionId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record OrganizationUnitUpdatedDomainEvent(
        Guid OrganizationUnitId,
        string Code,
        string Name,
        string InternationalName,
        string Type,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record OrganizationUnitManagerPositionChangedDomainEvent(
        Guid OrganizationUnitId,
        Guid? PreviousManagerPositionId,
        Guid? NewManagerPositionId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record OrganizationUnitMovedDomainEvent(
        Guid OrganizationUnitId,
        Guid? OldParentId,
        Guid? NewParentId,
        string NewHierarchyPath,
        int NewLevel,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record OrganizationUnitStatusChangedDomainEvent(
        Guid OrganizationUnitId,
        bool IsActive,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

    public sealed record OrganizationUnitDeletedDomainEvent(
        Guid OrganizationUnitId,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits
{
    public sealed partial class OrganizationUnit : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
    {
        private readonly List<OrganizationUnit> _children = [];

        private OrganizationUnit() { }

        public string ReferenceId { get; } = VillsourceId.Key;
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string InternationalName { get; private set; } = null!;
        public string Type { get; private set; } = null!;
        public string? Description { get; private set; }

        public Guid? LegalEntityId { get; private set; }
        public string? LegalEntityRefId { get; private set; }

        public Guid? ParentId { get; private set; }
        public OrganizationUnit? Parent { get; private set; } = null!;

        public int Level { get; private set; }
        public string HierarchyPath { get; private set; } = null!;

        public string? CostCenterCode { get; private set; }

        public Guid? ManagerPositionId { get; private set; }
        public Position? ManagerPosition { get; private set; }

        public bool IsActive { get; private set; } = true;

        public IReadOnlyCollection<OrganizationUnit> Children => _children.AsReadOnly();

        public static OrganizationUnit Create(
            string code,
            string name,
            string internationalName,
            string type,
            string? description = null,
            Guid? legalEntityId = null,
            string? legalEntityRefId = null,
            OrganizationUnit? parent = null,
            string? costCenterCode = null,
            Position? managerPosition = null,
            bool allowExternalManagerPosition = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
            ArgumentException.ThrowIfNullOrWhiteSpace(type);

            if (parent is not null && parent.IsDeleted)
            {
                throw new InvalidOperationException($"Cannot attach to deleted parent OrganizationUnit '{parent.ReferenceId}'.");
            }

            var entity = new OrganizationUnit
            {
                Id = Guid.CreateVersion7(),
                Code = code.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                InternationalName = internationalName.Trim(),
                Type = type.Trim().ToUpperInvariant(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                LegalEntityId = legalEntityId ?? parent?.LegalEntityId,
                LegalEntityRefId = string.IsNullOrWhiteSpace(legalEntityRefId) ? parent?.LegalEntityRefId : legalEntityRefId.Trim(),
                ParentId = parent?.Id,
                Level = parent is null ? 0 : parent.Level + 1,
                CostCenterCode = string.IsNullOrWhiteSpace(costCenterCode) ? null : costCenterCode.Trim().ToUpperInvariant(),
                IsActive = true
            };

            entity.HierarchyPath = parent is null
                ? $"/{entity.ReferenceId}/"
                : $"{parent.HierarchyPath}{entity.ReferenceId}/";

            if (managerPosition is not null)
            {
                ValidateManagerPositionCandidate(
                    targetOuId: entity.Id,
                    targetOuRefId: entity.ReferenceId,
                    candidate: managerPosition,
                    allowExternalManagerPosition: allowExternalManagerPosition);

                entity.ManagerPositionId = managerPosition.Id;
                entity.ManagerPosition = managerPosition;
            }

            entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitCreatedDomainEvent(
                    OrganizationUnitId: entity.Id,
                    Code: entity.Code,
                    Name: entity.Name,
                    InternationalName: entity.InternationalName,
                    Type: entity.Type,
                    LegalEntityId: entity.LegalEntityId,
                    ParentId: entity.ParentId,
                    ManagerPositionId: entity.ManagerPositionId,
                    EventId: id,
                    OccurredOnUtc: ts)));

            return entity;
        }

        public void Update(
            string code,
            string name,
            string internationalName,
            string type,
            string? description = null,
            string? costCenterCode = null)
        {
            EnsureNotDeleted();
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
            ArgumentException.ThrowIfNullOrWhiteSpace(type);

            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
            InternationalName = internationalName.Trim();
            Type = type.Trim().ToUpperInvariant();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            CostCenterCode = string.IsNullOrWhiteSpace(costCenterCode) ? null : costCenterCode.Trim().ToUpperInvariant();

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitUpdatedDomainEvent(
                    OrganizationUnitId: Id,
                    Code: Code,
                    Name: Name,
                    InternationalName: InternationalName,
                    Type: Type,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public void AssignManagerPosition(Position? managerPosition, bool allowExternalManagerPosition = false)
        {
            EnsureNotDeleted();

            var newManagerPositionId = managerPosition?.Id;
            if (ManagerPositionId == newManagerPositionId)
            {
                return;
            }

            if (managerPosition is not null)
            {
                ValidateManagerPositionCandidate(
                    targetOuId: Id,
                    targetOuRefId: ReferenceId,
                    candidate: managerPosition,
                    allowExternalManagerPosition: allowExternalManagerPosition);
            }

            var previousManagerPositionId = ManagerPositionId;
            ManagerPositionId = newManagerPositionId;
            ManagerPosition = managerPosition;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitManagerPositionChangedDomainEvent(
                    OrganizationUnitId: Id,
                    PreviousManagerPositionId: previousManagerPositionId,
                    NewManagerPositionId: newManagerPositionId,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public void MoveTo(OrganizationUnit? newParent, IReadOnlyCollection<OrganizationUnit> descendants)
        {
            EnsureNotDeleted();

            if (newParent is not null)
            {
                if (newParent.IsDeleted)
                {
                    throw new InvalidOperationException($"Target parent OrganizationUnit '{newParent.ReferenceId}' is deleted.");
                }

                if (newParent.Id == Id || newParent.HierarchyPath.StartsWith(HierarchyPath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Cannot move an OrganizationUnit under itself or one of its descendants.");
                }
            }

            var oldParentId = ParentId;
            var oldPath = HierarchyPath;
            var oldLevel = Level;

            ParentId = newParent?.Id;
            Level = newParent is null ? 0 : newParent.Level + 1;
            HierarchyPath = newParent is null
                ? $"/{ReferenceId}/"
                : $"{newParent.HierarchyPath}{ReferenceId}/";

            var levelDelta = Level - oldLevel;

            foreach (var descendant in descendants.Where(d => !d.IsDeleted && d.Id != Id))
            {
                descendant.ApplyAncestorMove(
                    oldAncestorPath: oldPath,
                    newAncestorPath: HierarchyPath,
                    levelDelta: levelDelta);
            }

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitMovedDomainEvent(
                    OrganizationUnitId: Id,
                    OldParentId: oldParentId,
                    NewParentId: ParentId,
                    NewHierarchyPath: HierarchyPath,
                    NewLevel: Level,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        internal void ApplyAncestorMove(string oldAncestorPath, string newAncestorPath, int levelDelta)
        {
            HierarchyPath = string.Concat(newAncestorPath, HierarchyPath.AsSpan(oldAncestorPath.Length));
            Level += levelDelta;
        }

        public void Activate()
        {
            EnsureNotDeleted();
            if (IsActive)
            {
                return;
            }

            IsActive = true;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitStatusChangedDomainEvent(
                    OrganizationUnitId: Id,
                    IsActive: true,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public void Deactivate()
        {
            EnsureNotDeleted();
            if (!IsActive)
            {
                return;
            }

            IsActive = false;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitStatusChangedDomainEvent(
                    OrganizationUnitId: Id,
                    IsActive: false,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        public void Delete(string? deletedBy = null)
        {
            if (IsDeleted)
            {
                return;
            }

            if (_children.Any(c => !c.IsDeleted))
            {
                throw new InvalidOperationException($"Cannot delete OrganizationUnit '{ReferenceId}' while it still has active child units.");
            }

            DeletedOnUtc = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
            IsDeleted = true;
            IsActive = false;
            ManagerPositionId = null;
            ManagerPosition = null;

            AddDomainEvent(DomainEvent.Create((id, ts) =>
                new OrganizationUnitDeletedDomainEvent(
                    OrganizationUnitId: Id,
                    EventId: id,
                    OccurredOnUtc: ts)));
        }

        private static void ValidateManagerPositionCandidate(
            Guid targetOuId,
            string targetOuRefId,
            Position candidate,
            bool allowExternalManagerPosition)
        {
            if (candidate.IsDeleted)
            {
                throw new InvalidOperationException($"Cannot assign deleted Position '{candidate.ReferenceId}' as manager of OrganizationUnit '{targetOuRefId}'.");
            }

            if (!candidate.IsActive)
            {
                throw new InvalidOperationException($"Cannot assign inactive Position '{candidate.ReferenceId}' as manager of OrganizationUnit '{targetOuRefId}'.");
            }

            if (!allowExternalManagerPosition && candidate.OrgUnitId != targetOuId)
            {
                throw new InvalidOperationException(
                    $"Position '{candidate.ReferenceId}' belongs to another OrganizationUnit. Enable 'AllowExternalManagerPosition' for matrix/shared leadership.");
            }
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException($"OrganizationUnit '{ReferenceId}' has already been deleted.");
            }
        }
    }

    internal static partial class OrganizationUnitRules
    {
        [GeneratedRegex(@"^[A-Z0-9\-_]{2,50}$", RegexOptions.CultureInvariant)]
        internal static partial Regex CodeRegex();

        [GeneratedRegex(@"^[A-Z0-9_]{2,30}$", RegexOptions.CultureInvariant)]
        internal static partial Regex TypeRegex();
    }
}

#endregion

#region 2. EF Core Persistence Configuration

namespace Villsource.FSH.Modules.Organization.Data.Configurations
{
    public sealed class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("OrganizationUnits");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.HasIndex(e => e.ReferenceId).IsUnique();

            builder.HasIndex(e => e.ParentId);
            builder.HasIndex(e => e.LegalEntityId);
            builder.HasIndex(e => e.ManagerPositionId);
            builder.HasIndex(e => e.HierarchyPath);

            builder.Property(e => e.ReferenceId).HasMaxLength(VillsourceId.KeySizes).IsRequired();
            builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Name).HasMaxLength(250).IsRequired();
            builder.Property(e => e.InternationalName).HasMaxLength(250).IsRequired();
            builder.Property(e => e.Type).HasMaxLength(30).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.LegalEntityRefId).HasMaxLength(VillsourceId.KeySizes);
            builder.Property(e => e.HierarchyPath).HasMaxLength(1000).IsRequired();
            builder.Property(e => e.CostCenterCode).HasMaxLength(50);

            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.ManagerPosition)
                .WithMany()
                .HasForeignKey(e => e.ManagerPositionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(e => e.Children)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

#endregion

#region 3. Mapperly, Specifications, Validators, Handlers & Minimal API Endpoints

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits
{
    #region Mappers

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class OrganizationUnitMapper
    {
        [MapProperty(nameof(OrganizationUnit.ReferenceId), nameof(OrganizationUnitDetailDto.RefId))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentRefId), Use = nameof(GetParentRefId))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentCode), Use = nameof(GetParentCode))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentName), Use = nameof(GetParentName))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionRefId), Use = nameof(GetManagerPositionRefId))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionCode), Use = nameof(GetManagerPositionCode))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionTitle), Use = nameof(GetManagerPositionTitle))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.Children), Use = nameof(MapChildren))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
        [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
        public static partial OrganizationUnitDetailDto ToDetailDto(this OrganizationUnit entity);

        [MapProperty(nameof(OrganizationUnit.ReferenceId), nameof(OrganizationUnitChildDto.RefId))]
        [MapPropertyFromSource(nameof(OrganizationUnitChildDto.ManagerPositionRefId), Use = nameof(GetManagerPositionRefId))]
        [MapPropertyFromSource(nameof(OrganizationUnitChildDto.ManagerPositionTitle), Use = nameof(GetManagerPositionTitle))]
        public static partial OrganizationUnitChildDto ToChildDto(this OrganizationUnit entity);

        private static string? GetParentRefId(OrganizationUnit entity) => entity.Parent?.ReferenceId;
        private static string? GetParentCode(OrganizationUnit entity) => entity.Parent?.Code;
        private static string? GetParentName(OrganizationUnit entity) => entity.Parent?.Name;
        private static string? GetManagerPositionRefId(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.ReferenceId : null;
        private static string? GetManagerPositionCode(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.Code : null;
        private static string? GetManagerPositionTitle(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.Title : null;
        private static DateTimeOffset GetCreatedOnUtc(OrganizationUnit entity) => entity.CreatedOnUtc;
        private static DateTimeOffset? GetLastModifiedOnUtc(OrganizationUnit entity) => entity.LastModifiedOnUtc;

        [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
        private static IReadOnlyList<OrganizationUnitChildDto> MapChildren(OrganizationUnit entity) =>
            entity.Children
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Code)
                .Select(c => c.ToChildDto())
                .ToList();
    }

    internal sealed record UpdateOrganizationUnitRequest(
        string Code,
        string Name,
        string InternationalName,
        string Type,
        string? Description = null,
        string? CostCenterCode = null);

    internal sealed record AssignManagerPositionRequest(
        string? ManagerPositionRefId,
        bool AllowExternalManagerPosition = false);

    internal sealed record MoveOrganizationUnitRequest(string? NewParentRefId);

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class OrganizationUnitRequestMapper
    {
        internal static partial UpdateOrganizationUnitCommand ToCommand(
            this UpdateOrganizationUnitRequest request,
            string refId);

        internal static partial AssignManagerPositionCommand ToCommand(
            this AssignManagerPositionRequest request,
            string refId);

        internal static partial MoveOrganizationUnitCommand ToCommand(
            this MoveOrganizationUnitRequest request,
            string refId);
    }

    #endregion

    #region Specifications

    public sealed class SearchOrganizationUnitsSpec : Specification<OrganizationUnit, OrganizationUnitSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<OrganizationUnit, object>>> SortMappings =
            new Dictionary<string, Expression<Func<OrganizationUnit, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = e => e.Code,
                ["name"] = e => e.Name,
                ["internationalName"] = e => e.InternationalName,
                ["type"] = e => e.Type,
                ["level"] = e => e.Level,
                ["createdOnUtc"] = e => e.CreatedOnUtc
            };

        public SearchOrganizationUnitsSpec(SearchOrganizationUnitsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.IsActive.HasValue)
            {
                Where(e => e.IsActive == request.IsActive.Value);
            }

            if (request.RootOnly)
            {
                Where(e => e.ParentId == null);
            }
            else if (!string.IsNullOrWhiteSpace(request.ParentRefId))
            {
                var parentRefId = request.ParentRefId.Trim();
                Where(e => e.Parent != null && e.Parent.ReferenceId == parentRefId);
            }

            if (request.VacantManagerOnly)
            {
                Where(e => e.ManagerPositionId == null || e.ManagerPosition == null || e.ManagerPosition.IsDeleted || !e.ManagerPosition.IsActive);
            }
            else if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
            {
                var managerPosRefId = request.ManagerPositionRefId.Trim();
                Where(e => e.ManagerPosition != null && !e.ManagerPosition.IsDeleted && e.ManagerPosition.ReferenceId == managerPosRefId);
            }

            if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
            {
                var legalEntityRefId = request.LegalEntityRefId.Trim();
                Where(e => e.LegalEntityRefId == legalEntityRefId);
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                var type = request.Type.Trim().ToUpperInvariant();
                Where(e => e.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                Where(e =>
                    EF.Functions.ILike(e.Code, $"%{keyword}%") ||
                    EF.Functions.ILike(e.Name, $"%{keyword}%") ||
                    EF.Functions.ILike(e.InternationalName, $"%{keyword}%") ||
                    (e.ManagerPosition != null && !e.ManagerPosition.IsDeleted && EF.Functions.ILike(e.ManagerPosition.Title, $"%{keyword}%")));
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () =>
                {
                    OrderBy(e => e.Level);
                    ThenBy(e => e.Code);
                },
                sortMappings: SortMappings);

            Select(e => new OrganizationUnitSummaryDto(
                e.ReferenceId,
                e.Code,
                e.Name,
                e.InternationalName,
                e.Type,
                e.LegalEntityRefId,
                e.Parent != null ? e.Parent.ReferenceId : null,
                e.Parent != null ? e.Parent.Name : null,
                e.Level,
                e.HierarchyPath,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.ReferenceId : null,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Code : null,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Title : null,
                e.IsActive,
                e.Children.Count(c => !c.IsDeleted)));
        }
    }

    public sealed class GetOrganizationUnitByRefIdSpec : Specification<OrganizationUnit>
    {
        public GetOrganizationUnitByRefIdSpec(string refId)
        {
            Where(e => e.ReferenceId == refId);
            Include(e => e.Parent!);
            Include(e => e.ManagerPosition!);
            Include(e => e.Children.Where(c => !c.IsDeleted));
            Include(c => c.ManagerPosition!);
        }
    }

    #endregion

    #region Validators

    public sealed class SearchOrganizationUnitsQueryValidator : AbstractValidator<SearchOrganizationUnitsQuery>
    {
        public SearchOrganizationUnitsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
            RuleFor(x => x.ParentRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ParentRefId));
            RuleFor(x => x.ManagerPositionRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ManagerPositionRefId));
            RuleFor(x => x.Type).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Type));
        }
    }

    public sealed class GetOrganizationUnitByRefIdQueryValidator : AbstractValidator<GetOrganizationUnitByRefIdQuery>
    {
        public GetOrganizationUnitByRefIdQueryValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        }
    }

    public sealed class GetOrganizationUnitLeadershipChainQueryValidator : AbstractValidator<GetOrganizationUnitLeadershipChainQuery>
    {
        public GetOrganizationUnitLeadershipChainQueryValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        }
    }

    public sealed class CreateOrganizationUnitCommandValidator : AbstractValidator<CreateOrganizationUnitCommand>
    {
        public CreateOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50)
                .Must(code => OrganizationUnitRules.CodeRegex().IsMatch(code.Trim().ToUpperInvariant()))
                .WithMessage("Code must be 2-50 uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);

            RuleFor(x => x.Type)
                .NotEmpty()
                .MaximumLength(30)
                .Must(type => OrganizationUnitRules.TypeRegex().IsMatch(type.Trim().ToUpperInvariant()))
                .WithMessage("Type must be 2-30 uppercase alphanumeric characters or underscores.");

            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
            RuleFor(x => x.ParentRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ParentRefId));
            RuleFor(x => x.ManagerPositionRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ManagerPositionRefId));
            RuleFor(x => x.CostCenterCode).MaximumLength(50);
        }
    }

    public sealed class UpdateOrganizationUnitCommandValidator : AbstractValidator<UpdateOrganizationUnitCommand>
    {
        public UpdateOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50)
                .Must(code => OrganizationUnitRules.CodeRegex().IsMatch(code.Trim().ToUpperInvariant()))
                .WithMessage("Code must be 2-50 uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);

            RuleFor(x => x.Type)
                .NotEmpty()
                .MaximumLength(30)
                .Must(type => OrganizationUnitRules.TypeRegex().IsMatch(type.Trim().ToUpperInvariant()))
                .WithMessage("Type must be 2-30 uppercase alphanumeric characters or underscores.");

            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.CostCenterCode).MaximumLength(50);
        }
    }

    public sealed class AssignManagerPositionCommandValidator : AbstractValidator<AssignManagerPositionCommand>
    {
        public AssignManagerPositionCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.ManagerPositionRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ManagerPositionRefId));
        }
    }

    public sealed class MoveOrganizationUnitCommandValidator : AbstractValidator<MoveOrganizationUnitCommand>
    {
        public MoveOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
            RuleFor(x => x.NewParentRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.NewParentRefId));
        }
    }

    public sealed class ActivateOrganizationUnitCommandValidator : AbstractValidator<ActivateOrganizationUnitCommand>
    {
        public ActivateOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        }
    }

    public sealed class DeactivateOrganizationUnitCommandValidator : AbstractValidator<DeactivateOrganizationUnitCommand>
    {
        public DeactivateOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        }
    }

    public sealed class DeleteOrganizationUnitCommandValidator : AbstractValidator<DeleteOrganizationUnitCommand>
    {
        public DeleteOrganizationUnitCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        }
    }

    #endregion

    #region Handlers

    public sealed class SearchOrganizationUnitsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchOrganizationUnitsQuery, PagedResponse<OrganizationUnitSummaryDto>>
    {
        public async ValueTask<PagedResponse<OrganizationUnitSummaryDto>> Handle(
            SearchOrganizationUnitsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var spec = new SearchOrganizationUnitsSpec(request: request);
            return await dbContext.OrganizationUnits
                .ApplySpecification(specification: spec)
                .ToPagedResponseAsync(pagination: request, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public sealed class GetOrganizationUnitByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetOrganizationUnitByRefIdQuery, OrganizationUnitDetailDto>
    {
        public async ValueTask<OrganizationUnitDetailDto> Handle(
            GetOrganizationUnitByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var spec = new GetOrganizationUnitByRefIdSpec(refId: request.RefId);
            var entity = await dbContext.OrganizationUnits
                             .ApplySpecification(specification: spec)
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

    public sealed class GetOrganizationUnitLeadershipChainQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetOrganizationUnitLeadershipChainQuery, OrganizationUnitLeadershipChainDto>
    {
        public async ValueTask<OrganizationUnitLeadershipChainDto> Handle(
            GetOrganizationUnitLeadershipChainQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var target = await dbContext.OrganizationUnits
                             .AsNoTracking()
                             .Where(e => e.ReferenceId == request.RefId)
                             .Select(e => new { e.ReferenceId, e.Code, e.Name, e.Level, e.HierarchyPath })
                             .FirstOrDefaultAsync(cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            var ancestorRefIds = target.HierarchyPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var nodes = await dbContext.OrganizationUnits
                .AsNoTracking()
                .Where(e => ancestorRefIds.Contains(e.ReferenceId))
                .OrderByDescending(e => e.Level)
                .Select(e => new OrganizationUnitLeadershipNodeDto(
                    target.Level - e.Level,
                    e.Level,
                    e.ReferenceId,
                    e.Code,
                    e.Name,
                    e.IsActive,
                    e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.ReferenceId : null,
                    e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Code : null,
                    e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Title : null,
                    e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.IsActive : null))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new OrganizationUnitLeadershipChainDto(
                TargetOuRefId: target.ReferenceId,
                TargetOuCode: target.Code,
                TargetOuName: target.Name,
                Chain: nodes);
        }
    }

    public sealed class CreateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreateOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            CreateOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            OrganizationUnit? parent = null;
            if (!string.IsNullOrWhiteSpace(request.ParentRefId))
            {
                parent = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.ParentRefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"Parent OrganizationUnit '{request.ParentRefId}' was not found.");
            }

            Guid? legalEntityId = null;
            if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
            {
                var legalEntity = await dbContext.LegalEntities
                                      .Where(le => le.ReferenceId == request.LegalEntityRefId)
                                      .Select(le => new { le.Id })
                                      .FirstOrDefaultAsync(cancellationToken)
                                      .ConfigureAwait(false)
                                  ?? throw new NotFoundException($"LegalEntity '{request.LegalEntityRefId}' was not found.");
                legalEntityId = legalEntity.Id;
            }

            Position? managerPosition = null;
            if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
            {
                managerPosition = await dbContext.Positions
                                      .FirstOrDefaultAsync(p => p.ReferenceId == request.ManagerPositionRefId, cancellationToken)
                                      .ConfigureAwait(false)
                                  ?? throw new NotFoundException($"Manager Position '{request.ManagerPositionRefId}' was not found.");
            }

            var entity = OrganizationUnit.Create(
                code: request.Code,
                name: request.Name,
                internationalName: request.InternationalName,
                type: request.Type,
                description: request.Description,
                legalEntityId: legalEntityId,
                legalEntityRefId: request.LegalEntityRefId,
                parent: parent,
                costCenterCode: request.CostCenterCode,
                managerPosition: managerPosition,
                allowExternalManagerPosition: request.AllowExternalManagerPosition);

            await dbContext.OrganizationUnits.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return entity.ReferenceId;
        }
    }

    public sealed class UpdateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdateOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            UpdateOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            entity.Update(
                code: request.Code,
                name: request.Name,
                internationalName: request.InternationalName,
                type: request.Type,
                description: request.Description,
                costCenterCode: request.CostCenterCode);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    public sealed class AssignManagerPositionCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<AssignManagerPositionCommand, string>
    {
        public async ValueTask<string> Handle(
            AssignManagerPositionCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            Position? managerPosition = null;
            if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
            {
                managerPosition = await dbContext.Positions
                                      .FirstOrDefaultAsync(p => p.ReferenceId == request.ManagerPositionRefId, cancellationToken)
                                      .ConfigureAwait(false)
                                  ?? throw new NotFoundException($"Position '{request.ManagerPositionRefId}' was not found.");
            }

            entity.AssignManagerPosition(
                managerPosition: managerPosition,
                allowExternalManagerPosition: request.AllowExternalManagerPosition);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    public sealed class MoveOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<MoveOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            MoveOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            OrganizationUnit? newParent = null;
            if (!string.IsNullOrWhiteSpace(request.NewParentRefId))
            {
                newParent = await dbContext.OrganizationUnits
                                .FirstOrDefaultAsync(e => e.ReferenceId == request.NewParentRefId, cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Target parent OrganizationUnit '{request.NewParentRefId}' was not found.");
            }

            var descendants = await dbContext.OrganizationUnits
                .Where(e => e.HierarchyPath.StartsWith(entity.HierarchyPath) && e.Id != entity.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.MoveTo(newParent: newParent, descendants: descendants);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    public sealed class ActivateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ActivateOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            ActivateOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            entity.Activate();

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    public sealed class DeactivateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<DeactivateOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            DeactivateOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            entity.Deactivate();

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    public sealed class DeleteOrganizationUnitCommandHandler(
        OrganizationDbContext dbContext,
        ICurrentUser currentUser)
        : IRequestHandler<DeleteOrganizationUnitCommand, string>
    {
        public async ValueTask<string> Handle(
            DeleteOrganizationUnitCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.OrganizationUnits
                             .Include(e => e.Children.Where(c => !c.IsDeleted))
                             .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                             .ConfigureAwait(false)
                         ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

            entity.Delete(deletedBy: currentUser.GetUserId().ToString());

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity.ReferenceId;
        }
    }

    #endregion

    #region Minimal API Endpoints

    public static class OrganizationUnitEndpoints
    {
        public static IEndpointRouteBuilder MapOrganizationUnitEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/organization-units")
                .WithTags("Organization Units")
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/", async (
                    [AsParameters] SearchOrganizationUnitsQuery query,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(request: query, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(result);
                })
                .WithName("SearchOrganizationUnits")
                .WithSummary("Search organization units")
                .WithDescription("Searches and paginates organization units with optional hierarchy, legal entity, manager position, and vacancy filters.")
                .Produces<PagedResponse<OrganizationUnitSummaryDto>>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/{refId}", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(
                        request: new GetOrganizationUnitByRefIdQuery(RefId: refId),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(result);
                })
                .WithName("GetOrganizationUnit")
                .WithSummary("Get organization unit by reference ID")
                .WithDescription("Retrieves detailed organization unit information including its designated manager position, parent, and active child units.")
                .Produces<OrganizationUnitDetailDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{refId}/leadership-chain", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(
                        request: new GetOrganizationUnitLeadershipChainQuery(RefId: refId),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(result);
                })
                .WithName("GetOrganizationUnitLeadershipChain")
                .WithSummary("Get bottom-up leadership position chain")
                .WithDescription("Returns ordered manager positions from the target organization unit up to the root unit so downstream approval/HR resolvers can handle vacant manager positions and skip-level escalation.")
                .Produces<OrganizationUnitLeadershipChainDto>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                    [FromBody] CreateOrganizationUnitCommand command,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var refId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.CreatedAtRoute(
                        routeName: "GetOrganizationUnit",
                        routeValues: new { refId },
                        value: refId);
                })
                .WithName("CreateOrganizationUnit")
                .WithSummary("Create an organization unit")
                .WithDescription("Creates a new organization unit (root or under an existing parent unit) and optionally assigns a manager position.")
                .Accepts<CreateOrganizationUnitCommand>("application/json")
                .Produces<string>(StatusCodes.Status201Created, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPut("/{refId}", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    [FromBody] UpdateOrganizationUnitRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = request.ToCommand(refId: refId);
                    var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(updatedRefId);
                })
                .WithName("UpdateOrganizationUnit")
                .WithSummary("Update an organization unit")
                .WithDescription("Updates core metadata of an existing organization unit.")
                .Accepts<UpdateOrganizationUnitRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPut("/{refId}/manager-position", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    [FromBody] AssignManagerPositionRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = request.ToCommand(refId: refId);
                    var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(updatedRefId);
                })
                .WithName("AssignOrganizationUnitManagerPosition")
                .WithSummary("Assign or clear manager position of an organization unit")
                .WithDescription("Designates an active position as manager of the organization unit (or clears it when managerPositionRefId is null).")
                .Accepts<AssignManagerPositionRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPatch("/{refId}/move", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    [FromBody] MoveOrganizationUnitRequest request,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = request.ToCommand(refId: refId);
                    var movedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(movedRefId);
                })
                .WithName("MoveOrganizationUnit")
                .WithSummary("Move an organization unit")
                .WithDescription("Reparents an organization unit to a new parent (or promotes to root when newParentRefId is null) and recalculates descendant hierarchy paths.")
                .Accepts<MoveOrganizationUnitRequest>("application/json")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPatch("/{refId}/activate", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var activatedRefId = await mediator.Send(
                        request: new ActivateOrganizationUnitCommand(RefId: refId),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(activatedRefId);
                })
                .WithName("ActivateOrganizationUnit")
                .WithSummary("Activate an organization unit")
                .WithDescription("Marks an organization unit as active.")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPatch("/{refId}/deactivate", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var deactivatedRefId = await mediator.Send(
                        request: new DeactivateOrganizationUnitCommand(RefId: refId),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(deactivatedRefId);
                })
                .WithName("DeactivateOrganizationUnit")
                .WithSummary("Deactivate an organization unit")
                .WithDescription("Marks an organization unit as inactive.")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete("/{refId}", async (
                    [Description("Unique reference ID of the organization unit.")] string refId,
                    ISender mediator,
                    CancellationToken cancellationToken) =>
                {
                    var deletedRefId = await mediator.Send(
                        request: new DeleteOrganizationUnitCommand(RefId: refId),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    return Results.Ok(deletedRefId);
                })
                .WithName("DeleteOrganizationUnit")
                .WithSummary("Delete an organization unit")
                .WithDescription("Soft-deletes an organization unit if it has no active child units.")
                .Produces<string>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }
    }

    #endregion
}

#endregion