using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

public sealed record GetJobFamilyLookupsQuery(bool ActiveOnly = true)
    : IRequest<IReadOnlyList<JobFamilyLookupDto>>;
