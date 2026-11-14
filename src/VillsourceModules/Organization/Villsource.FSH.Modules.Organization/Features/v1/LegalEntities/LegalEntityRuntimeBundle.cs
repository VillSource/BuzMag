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
using System.ComponentModel;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.ObjectValue;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

#region Specifications

public sealed class SearchLegalEntitiesSpec : Specification<LegalEntity, LegalEntitySummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<LegalEntity, object>>> SortMappings =
        new Dictionary<string, Expression<Func<LegalEntity, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = e => e.Code,
            ["countryCode"] = e => e.CountryCode,
            ["legalName"] = e => e.LegalName,
            ["internationalName"] = e => e.InternationalName,
            ["taxId"] = e => e.TaxId,
            ["createdOnUtc"] = e => e.CreatedOnUtc
        };

    public SearchLegalEntitiesSpec(SearchLegalEntitiesQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();
            Where(e => e.CountryCode == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(e =>
                EF.Functions.ILike(e.Code, $"%{keyword}%") ||
                EF.Functions.ILike(e.LegalName, $"%{keyword}%") ||
                EF.Functions.ILike(e.InternationalName, $"%{keyword}%") ||
                EF.Functions.ILike(e.TaxId, $"%{keyword}%") ||
                (e.Profile.TradeName != null && EF.Functions.ILike(e.Profile.TradeName, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(e => e.Code),
            sortMappings: SortMappings);

        Select(e => new LegalEntitySummaryDto(
            e.ReferenceId,
            e.Code,
            e.CountryCode,
            e.LegalName,
            e.InternationalName,
            e.TaxId,
            e.RegistrationNumber,
            e.BaseCurrency,
            e.Profile.TradeName,
            e.Profile.LogoUrl,
            e.Branches.Count(b => b.IsActive && !b.IsDeleted)));
    }
}

public sealed class GetLegalEntityByRefIdSpec : Specification<LegalEntity>
{
    public GetLegalEntityByRefIdSpec(string refId)
    {
        Where(e => e.ReferenceId == refId);
        Include(e => e.Branches);
    }
}

#endregion

#region Validators

public sealed class SearchLegalEntitiesQueryValidator : AbstractValidator<SearchLegalEntitiesQuery>
{
    public SearchLegalEntitiesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.CountryCode)
            .Length(2)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));
    }
}

public sealed class GetLegalEntityByRefIdQueryValidator : AbstractValidator<GetLegalEntityByRefIdQuery>
{
    public GetLegalEntityByRefIdQueryValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}

public sealed class CreateLegalEntityCommandValidator : AbstractValidator<CreateLegalEntityCommand>
{
    public CreateLegalEntityCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BaseCurrency).NotEmpty().Length(3);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);

        When(x => x.Profile is not null, () =>
        {
            RuleFor(x => x.Profile!.TradeName).MaximumLength(250);
            RuleFor(x => x.Profile!.LogoUrl).MaximumLength(500);
            RuleFor(x => x.Profile!.Website).MaximumLength(250);
            RuleFor(x => x.Profile!.ContactEmail).EmailAddress().MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Profile!.ContactEmail));
            RuleFor(x => x.Profile!.ContactPhone).MaximumLength(50);
        });

        When(x => x.HeadOfficeAddress is not null, () =>
        {
            RuleFor(x => x.HeadOfficeBranchCode).MaximumLength(20);
            RuleFor(x => x.HeadOfficeName).MaximumLength(250);
            RuleFor(x => x.HeadOfficeInternationalName).MaximumLength(250);
        });
    }
}

public sealed class UpdateLegalEntityCommandValidator : AbstractValidator<UpdateLegalEntityCommand>
{
    public UpdateLegalEntityCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BaseCurrency).NotEmpty().Length(3);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);
    }
}

public sealed class UpdateLegalEntityProfileCommandValidator : AbstractValidator<UpdateLegalEntityProfileCommand>
{
    public UpdateLegalEntityProfileCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.TradeName).MaximumLength(250);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.Website).MaximumLength(250);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(50);
    }
}

public sealed class DeleteLegalEntityCommandValidator : AbstractValidator<DeleteLegalEntityCommand>
{
    public DeleteLegalEntityCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}

public sealed class AddTaxBranchCommandValidator : AbstractValidator<AddTaxBranchCommand>
{
    public AddTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.RegisteredAddress).NotNull();
        RuleFor(x => x.TaxSubId).MaximumLength(30);
    }
}

public sealed class UpdateTaxBranchCommandValidator : AbstractValidator<UpdateTaxBranchCommand>
{
    public UpdateTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.RegisteredAddress).NotNull();
        RuleFor(x => x.TaxSubId).MaximumLength(30);
    }
}

public sealed class ActivateTaxBranchCommandValidator : AbstractValidator<ActivateTaxBranchCommand>
{
    public ActivateTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}

public sealed class CloseTaxBranchCommandValidator : AbstractValidator<CloseTaxBranchCommand>
{
    public CloseTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}

public sealed class RemoveTaxBranchCommandValidator : AbstractValidator<RemoveTaxBranchCommand>
{
    public RemoveTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}

#endregion

#region Query Handlers

public sealed class SearchLegalEntitiesQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<SearchLegalEntitiesQuery, PagedResponse<LegalEntitySummaryDto>>
{
    public async ValueTask<PagedResponse<LegalEntitySummaryDto>> Handle(
        SearchLegalEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new SearchLegalEntitiesSpec(request: request);

        return await dbContext.LegalEntities
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(
                pagination: request,
                cancellationToken: cancellationToken);
    }
}

public sealed class GetLegalEntityByRefIdQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<GetLegalEntityByRefIdQuery, LegalEntityDetailDto>
{
    public async ValueTask<LegalEntityDetailDto> Handle(
        GetLegalEntityByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new GetLegalEntityByRefIdSpec(refId: request.RefId);

        var entity = await dbContext.LegalEntities
                         .ApplySpecification(specification: spec)
                         .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                     ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}

#endregion

#region Command Handlers

public sealed class CreateLegalEntityCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CreateLegalEntityCommand, string>
{
    public async ValueTask<string> Handle(
        CreateLegalEntityCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        CompanyProfile? profile = request.Profile is null
            ? null
            : new CompanyProfile
            {
                TradeName = request.Profile.TradeName,
                LogoUrl = request.Profile.LogoUrl,
                Website = request.Profile.Website,
                ContactEmail = request.Profile.ContactEmail,
                ContactPhone = request.Profile.ContactPhone
            };

        var legalEntity = LegalEntity.Create(
            code: request.Code,
            countryCode: request.CountryCode,
            legalName: request.LegalName,
            internationalName: request.InternationalName,
            taxId: request.TaxId,
            baseCurrency: request.BaseCurrency,
            registrationNumber: request.RegistrationNumber,
            profile: profile,
            headOfficeAddress: request.HeadOfficeAddress,
            headOfficeBranchCode: request.HeadOfficeBranchCode,
            headOfficeName: request.HeadOfficeName,
            headOfficeInternationalName: request.HeadOfficeInternationalName,
            headOfficeRegisteredDate: request.HeadOfficeRegisteredDate);

        await dbContext.LegalEntities.AddAsync(
            entity: legalEntity,
            cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}

public sealed class UpdateLegalEntityCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateLegalEntityCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateLegalEntityCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.RefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        legalEntity.Update(
            code: request.Code,
            legalName: request.LegalName,
            internationalName: request.InternationalName,
            taxId: request.TaxId,
            baseCurrency: request.BaseCurrency,
            registrationNumber: request.RegistrationNumber);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}

public sealed class UpdateLegalEntityProfileCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateLegalEntityProfileCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateLegalEntityProfileCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.RefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        var profile = new CompanyProfile
        {
            TradeName = request.TradeName,
            LogoUrl = request.LogoUrl,
            Website = request.Website,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        legalEntity.UpdateProfile(profile: profile);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}

public sealed class DeleteLegalEntityCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteLegalEntityCommand, string>
{
    public async ValueTask<string> Handle(
        DeleteLegalEntityCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.RefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        legalEntity.Delete(deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}

public sealed class AddTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<AddTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        AddTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var branch = legalEntity.AddBranch(
            branchCode: request.BranchCode,
            name: request.Name,
            internationalName: request.InternationalName,
            registeredAddress: request.RegisteredAddress,
            isHeadOffice: request.IsHeadOffice,
            taxSubId: request.TaxSubId,
            registeredDate: request.RegisteredDate,
            isActive: request.IsActive);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return branch.ReferenceId;
    }
}

public sealed class UpdateTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        legalEntity.UpdateBranch(
            branchRef: request.RefId,
            name: request.Name,
            internationalName: request.InternationalName,
            registeredAddress: request.RegisteredAddress,
            taxSubId: request.TaxSubId,
            registeredDate: request.RegisteredDate);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return request.RefId;
    }
}

public sealed class ActivateTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<ActivateTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        ActivateTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        legalEntity.ActivateBranch(request.RefId);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return request.RefId;
    }
}

public sealed class CloseTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CloseTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        CloseTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        legalEntity.CloseBranch(
            branchRef: request.RefId,
            closedDate: request.ClosedDate);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return request.RefId;
    }
}

public sealed class RemoveTaxBranchCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        RemoveTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        legalEntity.RemoveBranch(
            branchRef: request.RefId,
            deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return request.RefId;
    }
}

#endregion

#region Minimal API & Endpoint Mapper Extension

public static class LegalEntityEndpoints
{
    public static IEndpointRouteBuilder MapLegalEntityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/legal-entities")
            .WithTags("Legal Entities")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Queries
        group.MapGet("/", async (
                [AsParameters] SearchLegalEntitiesQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchLegalEntities")
            .WithSummary("Search legal entities")
            .WithDescription(
                "Searches and paginates legal entities with optional keyword, country code filter, and sorting.")
            .Produces<PagedResponse<LegalEntitySummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetLegalEntityByRefIdQuery(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetLegalEntity")
            .WithSummary("Get legal entity by reference ID")
            .WithDescription(
                "Retrieves detailed legal entity information, company profile, and associated tax branches.")
            .Produces<LegalEntityDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // LegalEntity Commands
        group.MapPost("/", async (
                [FromBody] CreateLegalEntityCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.CreatedAtRoute(
                    routeName: "GetLegalEntity",
                    routeValues: new { refId },
                    value: refId);
            })
            .WithName("CreateLegalEntity")
            .WithSummary("Create a legal entity")
            .WithDescription(
                "Creates a new legal entity with optional company profile and head office tax branch, returning its reference ID.")
            .Accepts<CreateLegalEntityCommand>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                [FromBody] UpdateLegalEntityRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateLegalEntity")
            .WithSummary("Update a legal entity")
            .WithDescription("Updates core legal registration and tax details of an existing legal entity.")
            .Accepts<UpdateLegalEntityRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{refId}/profile", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                [FromBody] UpdateLegalEntityProfileRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateLegalEntityProfile")
            .WithSummary("Update legal entity profile")
            .WithDescription("Updates branding and contact profile metadata for an existing legal entity.")
            .Accepts<UpdateLegalEntityProfileRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deletedRefId = await mediator.Send(
                    request: new DeleteLegalEntityCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deletedRefId);
            })
            .WithName("DeleteLegalEntity")
            .WithSummary("Delete a legal entity")
            .WithDescription("Soft-deletes a legal entity and all of its associated tax branches.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // TaxBranch Sub-resource Commands
        group.MapPost("/{legalEntityRefId}/branches", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [FromBody] AddTaxBranchRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(legalEntityRefId: legalEntityRefId);
                var branchRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(branchRefId);
            })
            .WithName("AddTaxBranch")
            .WithSummary("Add a tax branch")
            .WithDescription(
                "Adds a new tax branch or establishment to an existing legal entity, returning the branch reference ID.")
            .Accepts<AddTaxBranchRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{legalEntityRefId}/branches/{refId}", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                [FromBody] UpdateTaxBranchRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(legalEntityRefId: legalEntityRefId, refId: refId);
                var updatedBranchRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedBranchRefId);
            })
            .WithName("UpdateTaxBranch")
            .WithSummary("Update a tax branch")
            .WithDescription("Updates registration details and address of an existing tax branch.")
            .Accepts<UpdateTaxBranchRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{legalEntityRefId}/branches/{refId}/activate", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var activatedRefId = await mediator.Send(
                    request: new ActivateTaxBranchCommand(LegalEntityRefId: legalEntityRefId, RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(activatedRefId);
            })
            .WithName("ActivateTaxBranch")
            .WithSummary("Activate a tax branch")
            .WithDescription("Re-activates a closed tax branch.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{legalEntityRefId}/branches/{refId}/close", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                [FromQuery, Description("Optional effective closure timestamp in UTC.")]
                DateTimeOffset? closedDate,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var closedRefId = await mediator.Send(
                    request: new CloseTaxBranchCommand(
                        LegalEntityRefId: legalEntityRefId,
                        RefId: refId,
                        ClosedDate: closedDate),
                    cancellationToken: cancellationToken);
                return Results.Ok(closedRefId);
            })
            .WithName("CloseTaxBranch")
            .WithSummary("Close a tax branch")
            .WithDescription("Closes an active tax branch with an optional effective closure date.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{legalEntityRefId}/branches/{refId}", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var removedRefId = await mediator.Send(
                    request: new RemoveTaxBranchCommand(LegalEntityRefId: legalEntityRefId, RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(removedRefId);
            })
            .WithName("RemoveTaxBranch")
            .WithSummary("Remove a tax branch")
            .WithDescription("Soft-deletes a tax branch from a legal entity.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}

#region HTTP Request Models & Mapper

internal sealed record UpdateLegalEntityRequest(
    string Code,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null);

internal sealed record UpdateLegalEntityProfileRequest(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone);

internal sealed record AddTaxBranchRequest(
    string BranchCode,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null,
    bool IsActive = true);

internal sealed record UpdateTaxBranchRequest(
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class LegalEntityRequestMapper
{
    internal static partial UpdateLegalEntityCommand ToCommand(
        this UpdateLegalEntityRequest request,
        string refId);

    internal static partial UpdateLegalEntityProfileCommand ToCommand(
        this UpdateLegalEntityProfileRequest request,
        string refId);

    internal static partial AddTaxBranchCommand ToCommand(
        this AddTaxBranchRequest request,
        string legalEntityRefId,
        bool isHeadOffice = false);

    internal static partial UpdateTaxBranchCommand ToCommand(
        this UpdateTaxBranchRequest request,
        string legalEntityRefId,
        string refId);
}

#endregion

#endregion