using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

public sealed record ActivateJobFamilyCommand(string RefId) : IRequest<Unit>;
