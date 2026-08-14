using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record UpdatePositionCommand(
    string PositionId,
    string Code,
    string Name,
    string? Description = null) : ICommand<PositionDto>;