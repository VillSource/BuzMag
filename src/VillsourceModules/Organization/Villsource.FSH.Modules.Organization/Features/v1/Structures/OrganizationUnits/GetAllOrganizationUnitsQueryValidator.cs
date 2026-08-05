using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class GetAllOrganizationUnitsQueryValidator : AbstractValidator<GetAllOrganizationUnitsQuery>
{
    public GetAllOrganizationUnitsQueryValidator()
    {
    }
}