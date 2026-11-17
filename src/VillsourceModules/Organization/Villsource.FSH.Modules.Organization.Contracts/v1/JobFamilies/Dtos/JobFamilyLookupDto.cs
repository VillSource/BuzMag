namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

public sealed record JobFamilyLookupDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName);
