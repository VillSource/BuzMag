using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;

public sealed record DeleteJobGradeCommand(string RefId) : IRequest<string>;