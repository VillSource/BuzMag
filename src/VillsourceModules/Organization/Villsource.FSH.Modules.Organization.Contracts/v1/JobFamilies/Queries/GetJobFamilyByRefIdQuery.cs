using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

public sealed record GetJobFamilyByRefIdQuery(string RefId) : IRequest<JobFamilyDetailDto>;
