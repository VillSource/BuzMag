using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;

public sealed record ActivateJobGradeCommand(string RefId) : IRequest<string>;