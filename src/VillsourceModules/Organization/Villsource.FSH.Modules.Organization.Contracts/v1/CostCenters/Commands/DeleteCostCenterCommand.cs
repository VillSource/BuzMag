using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

public sealed record DeleteCostCenterCommand(string RefId) : IRequest<string>;