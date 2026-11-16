using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

public sealed record DeactivateCostCenterCommand(string RefId) : IRequest<string>;