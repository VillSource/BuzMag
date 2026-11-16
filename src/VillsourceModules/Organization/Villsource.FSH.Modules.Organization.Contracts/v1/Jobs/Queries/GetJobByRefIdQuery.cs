using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;

public sealed record GetJobByRefIdQuery(string RefId) : IRequest<JobDetailDto>;