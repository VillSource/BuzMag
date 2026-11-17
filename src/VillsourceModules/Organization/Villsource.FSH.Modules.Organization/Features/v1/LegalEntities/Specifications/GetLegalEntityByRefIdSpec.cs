using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class GetLegalEntityByRefIdSpec : Specification<LegalEntity>
{
    public GetLegalEntityByRefIdSpec(string refId)
    {
        Where(e => e.ReferenceId == refId);
        Include(e => e.Branches);
    }
}