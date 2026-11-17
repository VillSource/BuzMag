using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

public sealed record DeactivateJobFamilyCommand(string RefId) : IRequest<Unit>;
