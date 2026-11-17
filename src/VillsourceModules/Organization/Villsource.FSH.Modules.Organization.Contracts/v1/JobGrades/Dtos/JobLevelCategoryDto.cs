namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

public sealed record JobLevelCategoryDto(
    string Key,
    string DisplayName,
    int SortOrder);
