using System.ComponentModel;
using System.Linq.Expressions;
using FluentValidation;
using FSH.Framework.Core.Context;
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
using Riok.Mapperly.Abstractions;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;

#region Specifications

public sealed class SearchCostCentersSpec : Specification<CostCenter, CostCenterSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<CostCenter, object>>> SortMappings =
        new Dictionary<string, Expression<Func<CostCenter, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = c => c.Code,
            ["name"] = c => c.Name,
            ["internationalName"] = c => c.InternationalName,
            ["functionType"] = c => c.FunctionType,
            ["isActive"] = c => c.IsActive,
            ["isPostingBlocked"] = c => c.IsPostingBlocked,
            ["validFrom"] = c => c.ValidFrom!,
            ["validTo"] = c => c.ValidTo!,
            ["legalEntityName"] = c => c.LegalEntity!.LegalName,
            ["createdOnUtc"] = c => c.CreatedOnUtc
        };

    public SearchCostCentersSpec(SearchCostCentersQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var legalEntityRefId = request.LegalEntityRefId.Trim();
            Where(c => c.LegalEntity!.ReferenceId == legalEntityRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.ParentCostCenterRefId))
        {
            var parentRefId = request.ParentCostCenterRefId.Trim();
            Where(c => c.ParentCostCenter != null && c.ParentCostCenter.ReferenceId == parentRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.FunctionType))
        {
            var functionType = CostCenterFunctionType.FromKey(request.FunctionType);
            Where(c => c.FunctionType == functionType);
        }

        if (request.ResponsibleUserId.HasValue)
        {
            Where(c => c.ResponsibleUserId == request.ResponsibleUserId.Value);
        }

        if (request.OrgUnitId.HasValue)
        {
            Where(c => c.OrgUnitId == request.OrgUnitId.Value);
        }

        if (request.IsActive.HasValue)
        {
            Where(c => c.IsActive == request.IsActive.Value);
        }

        if (request.IsPostingBlocked.HasValue)
        {
            Where(c => c.IsPostingBlocked == request.IsPostingBlocked.Value);
        }

        if (request.EffectiveAtUtc.HasValue)
        {
            var utcTarget = request.EffectiveAtUtc.Value.ToUniversalTime();
            Where(c =>
                (c.ValidFrom == null || c.ValidFrom <= utcTarget) &&
                (c.ValidTo == null || c.ValidTo >= utcTarget));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(c =>
                EF.Functions.ILike(c.Code, $"%{keyword}%") ||
                EF.Functions.ILike(c.Name, $"%{keyword}%") ||
                EF.Functions.ILike(c.InternationalName, $"%{keyword}%") ||
                (c.Description != null && EF.Functions.ILike(c.Description, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(c => c.Code),
            sortMappings: SortMappings);

        Select(c => new CostCenterSummaryDto(
            c.ReferenceId,
            c.LegalEntity!.ReferenceId,
            c.LegalEntity!.LegalName,
            c.ParentCostCenter != null ? c.ParentCostCenter.ReferenceId : null,
            c.ParentCostCenter != null ? c.ParentCostCenter.Code : null,
            c.Code,
            c.Name,
            c.InternationalName,
            (string)c.FunctionType,
            c.DefaultTaxBranch != null ? c.DefaultTaxBranch.BranchCode : null,
            c.ResponsibleUserId,
            c.OrgUnitId,
            c.ValidFrom,
            c.ValidTo,
            c.IsActive,
            c.IsPostingBlocked,
            c.Children.Count(child => !child.IsDeleted && child.IsActive)));
    }
}

public sealed class GetCostCenterByRefIdSpec : Specification<CostCenter>
{
    public GetCostCenterByRefIdSpec(string refId)
    {
        Where(c => c.ReferenceId == refId);
        Include(c => c.LegalEntity!);
        Include(c => c.ParentCostCenter!);
        Include(c => c.DefaultTaxBranch!);
        Include(c => c.Children.Where(child => !child.IsDeleted));
    }
}

#endregion

#region Validators (DRY Shared Extensions)

internal static class CostCenterValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidCostCenterCode<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(50)
            .Must(CostCenterRules.IsValidCode)
            .WithMessage("Code must be 2-50 alphanumeric characters, hyphens, or underscores.");

    public static IRuleBuilderOptions<T, string> ValidFunctionTypeKey<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .Must(CostCenterFunctionType.IsValidKey)
            .WithMessage($"FunctionType must be one of: {CostCenterFunctionType.AllowedKeysDescription}.");

    public static IRuleBuilderOptions<T, string> ValidRefId<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(VillsourceId.KeySizes);
}

public sealed class SearchCostCentersQueryValidator : AbstractValidator<SearchCostCentersQuery>
{
    public SearchCostCentersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Sort)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));

        RuleFor(x => x.LegalEntityRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));

        RuleFor(x => x.ParentCostCenterRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ParentCostCenterRefId));

        RuleFor(x => x.FunctionType)
            .Must(CostCenterFunctionType.IsValidKey)
            .WithMessage($"FunctionType must be one of: {CostCenterFunctionType.AllowedKeysDescription}.")
            .When(x => !string.IsNullOrWhiteSpace(x.FunctionType));
    }
}

public sealed class GetCostCenterByRefIdQueryValidator : AbstractValidator<GetCostCenterByRefIdQuery>
{
    public GetCostCenterByRefIdQueryValidator() =>
        RuleFor(x => x.RefId).ValidRefId();
}

public sealed class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).ValidRefId();
        RuleFor(x => x.Code).ValidCostCenterCode();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.FunctionType).ValidFunctionTypeKey();

        RuleFor(x => x.ParentCostCenterRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ParentCostCenterRefId));

        RuleFor(x => x.DefaultTaxBranchRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultTaxBranchRefId));

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
            .WithMessage("ValidTo must be greater than or equal to ValidFrom.");
    }
}

public sealed class UpdateCostCenterCommandValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterCommandValidator()
    {
        RuleFor(x => x.RefId).ValidRefId();
        RuleFor(x => x.Code).ValidCostCenterCode();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.FunctionType).ValidFunctionTypeKey();

        RuleFor(x => x.ParentCostCenterRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .NotEqual(x => x.RefId)
            .WithMessage("A cost center cannot be its own parent.")
            .When(x => !string.IsNullOrWhiteSpace(x.ParentCostCenterRefId));

        RuleFor(x => x.DefaultTaxBranchRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultTaxBranchRefId));

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
            .WithMessage("ValidTo must be greater than or equal to ValidFrom.");
    }
}

public sealed class ActivateCostCenterCommandValidator : AbstractValidator<ActivateCostCenterCommand>
{
    public ActivateCostCenterCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

public sealed class DeactivateCostCenterCommandValidator : AbstractValidator<DeactivateCostCenterCommand>
{
    public DeactivateCostCenterCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

public sealed class BlockCostCenterPostingCommandValidator : AbstractValidator<BlockCostCenterPostingCommand>
{
    public BlockCostCenterPostingCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

public sealed class UnblockCostCenterPostingCommandValidator : AbstractValidator<UnblockCostCenterPostingCommand>
{
    public UnblockCostCenterPostingCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

public sealed class DeleteCostCenterCommandValidator : AbstractValidator<DeleteCostCenterCommand>
{
    public DeleteCostCenterCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

#endregion

#region Query Handlers

public sealed class SearchCostCentersQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<SearchCostCentersQuery, PagedResponse<CostCenterSummaryDto>>
{
    public async ValueTask<PagedResponse<CostCenterSummaryDto>> Handle(
        SearchCostCentersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new SearchCostCentersSpec(request: request);

        return await dbContext.CostCenters
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(
                pagination: request,
                cancellationToken: cancellationToken);
    }
}

public sealed class GetCostCenterByRefIdQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<GetCostCenterByRefIdQuery, CostCenterDetailDto>
{
    public async ValueTask<CostCenterDetailDto> Handle(
        GetCostCenterByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new GetCostCenterByRefIdSpec(refId: request.RefId);

        var entity = await dbContext.CostCenters
                         .ApplySpecification(specification: spec)
                         .AsSplitQuery()
                         .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                     ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}

public sealed class GetCostCenterFunctionTypesQueryHandler
    : IRequestHandler<GetCostCenterFunctionTypesQuery, IReadOnlyList<CostCenterFunctionTypeDto>>
{
    private static readonly IReadOnlyList<CostCenterFunctionTypeDto> CachedItems =
        CostCenterFunctionType.Items.Select(x => x.ToDto()).ToList();

    public ValueTask<IReadOnlyList<CostCenterFunctionTypeDto>> Handle(
        GetCostCenterFunctionTypesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ValueTask.FromResult(CachedItems);
    }
}

#endregion

#region Command Handlers & Shared Resolution Helpers

internal static class CostCenterReferenceResolver
{
    public static async Task<Guid?> ResolveParentCostCenterIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? parentCostCenterRefId,
        bool requireActiveParent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentCostCenterRefId))
        {
            return null;
        }

        var parent = await dbContext.CostCenters
            .AsNoTracking()
            .Where(c => c.ReferenceId == parentCostCenterRefId)
            .Select(c => new { c.Id, c.LegalEntityId, c.IsActive })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(
                $"Parent cost center '{parentCostCenterRefId}' was not found.");

        if (parent.LegalEntityId != legalEntityId)
        {
            throw new InvalidOperationException(
                "Parent cost center must belong to the same legal entity.");
        }

        if (requireActiveParent && !parent.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign an inactive parent cost center to an active cost center.");
        }

        return parent.Id;
    }

    public static async Task<Guid?> ResolveDefaultTaxBranchIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? defaultTaxBranchRefId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(defaultTaxBranchRefId))
        {
            return null;
        }

        var branch = await dbContext.Set<TaxBranch>()
            .AsNoTracking()
            .Where(b => b.LegalEntityId == legalEntityId && b.ReferenceId == defaultTaxBranchRefId)
            .Select(b => new { b.Id, b.IsActive })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(
                $"Tax branch '{defaultTaxBranchRefId}' was not found in this legal entity.");

        if (!branch.IsActive)
        {
            throw new InvalidOperationException(
                $"Tax branch '{defaultTaxBranchRefId}' is closed and cannot be set as default.");
        }

        return branch.Id;
    }

    public static async Task EnsureNoHierarchyCycleAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        Guid costCenterId,
        Guid proposedParentId,
        CancellationToken cancellationToken)
    {
        if (costCenterId == proposedParentId)
        {
            throw new InvalidOperationException("A cost center cannot be its own parent.");
        }

        // Single-query lookup for all nodes in the same LegalEntity (avoids N+1 DB round-trips)
        var parentMap = await dbContext.CostCenters
            .AsNoTracking()
            .Where(c => c.LegalEntityId == legalEntityId && c.ParentCostCenterId != null)
            .Select(c => new { c.Id, ParentId = c.ParentCostCenterId!.Value })
            .ToDictionaryAsync(x => x.Id, x => x.ParentId, cancellationToken: cancellationToken);

        var visited = new HashSet<Guid> { costCenterId };
        Guid? cursor = proposedParentId;

        while (cursor.HasValue)
        {
            if (!visited.Add(cursor.Value))
            {
                throw new InvalidOperationException(
                    "Assigning this parent cost center would create a circular hierarchy.");
            }

            cursor = parentMap.TryGetValue(cursor.Value, out var nextParentId)
                ? nextParentId
                : null;
        }
    }
}

public sealed class CreateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CreateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        CreateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntityId = await dbContext.LegalEntities
            .AsNoTracking()
            .Where(le => le.ReferenceId == request.LegalEntityRefId)
            .Select(le => (Guid?)le.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(request.Code);

        var codeExists = await dbContext.CostCenters
            .AnyAsync(
                predicate: c => c.LegalEntityId == legalEntityId && c.Code == normalizedCode,
                cancellationToken: cancellationToken);

        if (codeExists)
        {
            throw new CustomException(
                $"Cost center code '{normalizedCode}' already exists in legal entity '{request.LegalEntityRefId}'.", [], HttpStatusCode.Conflict);
        }

        var parentCostCenterId = await CostCenterReferenceResolver.ResolveParentCostCenterIdAsync(
            dbContext: dbContext,
            legalEntityId: legalEntityId,
            parentCostCenterRefId: request.ParentCostCenterRefId,
            requireActiveParent: request.IsActive,
            cancellationToken: cancellationToken);

        var defaultTaxBranchId = await CostCenterReferenceResolver.ResolveDefaultTaxBranchIdAsync(
            dbContext: dbContext,
            legalEntityId: legalEntityId,
            defaultTaxBranchRefId: request.DefaultTaxBranchRefId,
            cancellationToken: cancellationToken);

        var costCenter = CostCenter.Create(
            legalEntityId: legalEntityId,
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            functionType: CostCenterFunctionType.FromKey(request.FunctionType),
            description: request.Description,
            parentCostCenterId: parentCostCenterId,
            defaultTaxBranchId: defaultTaxBranchId,
            responsibleUserId: request.ResponsibleUserId,
            orgUnitId: request.OrgUnitId,
            validFrom: request.ValidFrom,
            validTo: request.ValidTo,
            isActive: request.IsActive,
            isPostingBlocked: request.IsPostingBlocked);

        await dbContext.CostCenters.AddAsync(
            entity: costCenter,
            cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class UpdateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(request.Code);

        if (!string.Equals(costCenter.Code, normalizedCode, StringComparison.Ordinal))
        {
            var codeExists = await dbContext.CostCenters
                .AnyAsync(
                    predicate: c => c.LegalEntityId == costCenter.LegalEntityId &&
                                    c.Code == normalizedCode &&
                                    c.Id != costCenter.Id,
                    cancellationToken: cancellationToken);

            if (codeExists)
            {
                throw new CustomException(
                    $"Cost center code '{normalizedCode}' already exists in this legal entity.", [], HttpStatusCode.Conflict);
            }
        }

        var parentCostCenterId = await CostCenterReferenceResolver.ResolveParentCostCenterIdAsync(
            dbContext: dbContext,
            legalEntityId: costCenter.LegalEntityId,
            parentCostCenterRefId: request.ParentCostCenterRefId,
            requireActiveParent: costCenter.IsActive,
            cancellationToken: cancellationToken);

        if (parentCostCenterId.HasValue)
        {
            await CostCenterReferenceResolver.EnsureNoHierarchyCycleAsync(
                dbContext: dbContext,
                legalEntityId: costCenter.LegalEntityId,
                costCenterId: costCenter.Id,
                proposedParentId: parentCostCenterId.Value,
                cancellationToken: cancellationToken);
        }

        var defaultTaxBranchId = await CostCenterReferenceResolver.ResolveDefaultTaxBranchIdAsync(
            dbContext: dbContext,
            legalEntityId: costCenter.LegalEntityId,
            defaultTaxBranchRefId: request.DefaultTaxBranchRefId,
            cancellationToken: cancellationToken);

        costCenter.Update(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            functionType: CostCenterFunctionType.FromKey(request.FunctionType),
            description: request.Description,
            parentCostCenterId: parentCostCenterId,
            defaultTaxBranchId: defaultTaxBranchId,
            responsibleUserId: request.ResponsibleUserId,
            orgUnitId: request.OrgUnitId,
            validFrom: request.ValidFrom,
            validTo: request.ValidTo);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class ActivateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<ActivateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        ActivateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .Include(c => c.ParentCostCenter)
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.Activate();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class DeactivateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<DeactivateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        DeactivateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .Include(c => c.Children.Where(child => !child.IsDeleted))
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class BlockCostCenterPostingCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<BlockCostCenterPostingCommand, string>
{
    public async ValueTask<string> Handle(
        BlockCostCenterPostingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.BlockPosting();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class UnblockCostCenterPostingCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UnblockCostCenterPostingCommand, string>
{
    public async ValueTask<string> Handle(
        UnblockCostCenterPostingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.UnblockPosting();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

public sealed class DeleteCostCenterCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        DeleteCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .Include(c => c.Children.Where(child => !child.IsDeleted))
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.Delete(deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

#endregion

#region Minimal API & Endpoint Mapper Extension

public static class CostCenterEndpoints
{
    public static IEndpointRouteBuilder MapCostCenterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cost-centers")
            .WithTags("Cost Centers")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Queries
        group.MapGet("/", async (
                [AsParameters] SearchCostCentersQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchCostCenters")
            .WithSummary("Search cost centers")
            .WithDescription(
                "Searches and paginates cost centers with optional filters for legal entity, parent hierarchy, accounting function type, validity window, and status.")
            .Produces<PagedResponse<CostCenterSummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/function-types", async (
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetCostCenterFunctionTypesQuery(),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCostCenterFunctionTypes")
            .WithSummary("List accounting function types")
            .WithDescription(
                "Retrieves standard IAS 1 / TFRS 1 functional cost classifications and their financial statement mapping metadata.")
            .Produces<IReadOnlyList<CostCenterFunctionTypeDto>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetCostCenterByRefIdQuery(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCostCenter")
            .WithSummary("Get cost center by reference ID")
            .WithDescription(
                "Retrieves detailed cost center information including hierarchy links, accounting function metadata, default tax branch, and child cost centers.")
            .Produces<CostCenterDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Commands
        group.MapPost("/", async (
                [FromBody] CreateCostCenterCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.CreatedAtRoute(
                    routeName: "GetCostCenter",
                    routeValues: new { refId },
                    value: refId);
            })
            .WithName("CreateCostCenter")
            .WithSummary("Create a cost center")
            .WithDescription("Creates a new cost center under a legal entity and returns its reference ID.")
            .Accepts<CreateCostCenterCommand>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                [FromBody] UpdateCostCenterRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateCostCenter")
            .WithSummary("Update a cost center")
            .WithDescription("Updates master attributes, hierarchy, accounting function type, and validity window of an existing cost center.")
            .Accepts<UpdateCostCenterRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var activatedRefId = await mediator.Send(
                    request: new ActivateCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(activatedRefId);
            })
            .WithName("ActivateCostCenter")
            .WithSummary("Activate a cost center")
            .WithDescription("Marks an inactive cost center as active if its parent cost center is active.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deactivatedRefId = await mediator.Send(
                    request: new DeactivateCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deactivatedRefId);
            })
            .WithName("DeactivateCostCenter")
            .WithSummary("Deactivate a cost center")
            .WithDescription("Marks an active cost center as inactive if no active child cost centers exist.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/block-posting", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var blockedRefId = await mediator.Send(
                    request: new BlockCostCenterPostingCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(blockedRefId);
            })
            .WithName("BlockCostCenterPosting")
            .WithSummary("Block cost center posting")
            .WithDescription("Locks new financial postings to the cost center while keeping it visible for reporting.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/unblock-posting", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var unblockedRefId = await mediator.Send(
                    request: new UnblockCostCenterPostingCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(unblockedRefId);
            })
            .WithName("UnblockCostCenterPosting")
            .WithSummary("Unblock cost center posting")
            .WithDescription("Re-allows financial postings to a posting-blocked cost center.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deletedRefId = await mediator.Send(
                    request: new DeleteCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deletedRefId);
            })
            .WithName("DeleteCostCenter")
            .WithSummary("Delete a cost center")
            .WithDescription("Soft-deletes a cost center if no active child cost centers exist.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}

#region HTTP Request Models & Mapper

internal sealed record UpdateCostCenterRequest(
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    string? Description = null,
    string? ParentCostCenterRefId = null,
    string? DefaultTaxBranchRefId = null,
    Guid? ResponsibleUserId = null,
    Guid? OrgUnitId = null,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class CostCenterRequestMapper
{
    internal static partial UpdateCostCenterCommand ToCommand(
        this UpdateCostCenterRequest request,
        string refId);
}

#endregion

#endregion