using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.ObjectValue;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record CreateNewEmployeeCommand(
    string Title,
    string FirstName,
    string? MiddleName,
    string LastName,
    string TitleEn,
    string FirstNameEn,
    string? MiddleNameEn,
    string LastNameEn,
    Address Address, 
    string Email
) : ICommand<EmployeeDto>;