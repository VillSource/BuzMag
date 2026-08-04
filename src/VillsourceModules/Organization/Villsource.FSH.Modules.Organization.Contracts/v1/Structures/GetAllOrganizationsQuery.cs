using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed class GetAllOrganizationsQuery() : IQuery<ICollection<OrganizationDto>>;