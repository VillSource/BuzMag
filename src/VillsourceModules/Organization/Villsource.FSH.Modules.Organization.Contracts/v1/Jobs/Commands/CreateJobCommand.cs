using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;

public sealed record CreateJobCommand(
    string Code,
    string Name,
    string InternationalName,
    string DefaultJobGradeRefId,
    string? JobFamily = null,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;