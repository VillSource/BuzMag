using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;
using Villsource.Modules.HumanResource.Services;

namespace Villsource.Modules.HumanResource.Features.v1.Employ.NewEmployee;

public class CreateNewEmployeeCommandHandler(HumanResourceDbContext dbContext, IEmployeeCodeFactory codeFactory)
    : ICommandHandler<CreateNewEmployeeCommand, EmployeeDto>
{
    public async ValueTask<EmployeeDto> Handle(CreateNewEmployeeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        string code  = await codeFactory.Create().ConfigureAwait(false);
        Employee employee = Employee.Create(
            code: code, 
            title: command.Title,
            titleEn: command.TitleEn,
            firstName: command.FirstName,
            firstNameEn: command.FirstNameEn,
            lastName: command.LastName,
            lastNameEn:command.LastNameEn,
            email: command.Email,
            address: command.Address);
        employee.SetMiddleName(command.MiddleName, command.MiddleNameEn);

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        EmployeeDto dto = employee.ToDetailDto();
        
        employee.MapAuditableFieldsTo(ref dto);

        return dto;
    }
}