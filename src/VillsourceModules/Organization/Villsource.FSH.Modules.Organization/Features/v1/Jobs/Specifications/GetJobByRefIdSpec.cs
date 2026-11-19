using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs.Specifications;

public sealed class GetJobByRefIdSpec : Specification<Job>
{
    public GetJobByRefIdSpec(string refId)
    {
        Where(j => j.ReferenceId == refId);
        Include(j => j.DefaultJobGrade!);
        Include(j => j.JobFamily!);
    }
}