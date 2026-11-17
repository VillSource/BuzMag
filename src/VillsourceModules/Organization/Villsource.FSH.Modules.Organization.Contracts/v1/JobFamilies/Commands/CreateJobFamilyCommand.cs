using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

public sealed record CreateJobFamilyCommand(
    string Code,
    string Name,
    string InternationalName,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;
