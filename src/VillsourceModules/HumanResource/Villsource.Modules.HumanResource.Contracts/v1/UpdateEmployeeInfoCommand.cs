using Mediator;
using System.Text.Json.Serialization;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.ObjectValue;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record UpdateEmployeeInfoCommand(
    [property: JsonIgnore] string EmployeeRef,
    string Title,
    string FirstName,
    string? MiddleName,
    string LastName,
    string TitleEn,
    string FirstNameEn,
    string? MiddleNameEn,
    string LastNameEn,
    Address? Address
) : ICommand<EmployeeDto>;