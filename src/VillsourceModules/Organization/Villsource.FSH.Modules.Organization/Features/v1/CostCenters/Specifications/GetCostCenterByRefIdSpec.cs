using FluentValidation;
using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Specifications;

public sealed class GetCostCenterByRefIdSpec : Specification<CostCenter>
{
    public GetCostCenterByRefIdSpec(string refId)
    {
        Where(c => c.ReferenceId == refId);
        Include(c => c.LegalEntity!);
        Include(c => c.ParentCostCenter!);
        Include(c => c.DefaultTaxBranch!);
        Include(c => c.ResponsiblePosition!);
        Include(c => c.Children.Where(child => !child.IsDeleted));
    }
}

