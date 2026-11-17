using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;

public sealed record GetJobLookupsQuery(
    bool ActiveOnly = true,
    string? JobFamilyRefId = null) : IRequest<IReadOnlyList<JobLookupDto>>;