using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Services;

public interface IOrganizationUnitService
{
    Task<List<OrganizationUnit>> GetDescendantsAsync(OrganizationUnit ancestor, CancellationToken ct = default);
}