using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

public sealed record UpdateJobFamilyCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? Description = null) : IRequest<string>;
