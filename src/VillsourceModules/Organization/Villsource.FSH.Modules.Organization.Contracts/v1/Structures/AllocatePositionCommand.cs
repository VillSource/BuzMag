using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record AllocatePositionCommand(
    string OrganizationUnitId,
    string PositionId,
    int? HeadCount)
    : ICommand<PositionAllocationDto>;