using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

public sealed record DeleteJobFamilyCommand(string RefId) : IRequest<Unit>;
