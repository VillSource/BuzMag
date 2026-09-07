using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Employ.UpdateEmployeeInfo;

public class UpdateEmployeeInfoCommandHandler(HumanResourceDbContext dbContext) : ICommandHandler<UpdateEmployeeInfoCommand, EmployeeDto>
{
    public async ValueTask<EmployeeDto> Handle(UpdateEmployeeInfoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Employee employee = await dbContext.Employees
                                .Where(e => e.Ref == command.EmployeeRef)
                                .SingleOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Employee not found for '{command.EmployeeRef}'");

        employee.UpdatePersonalInfo(
            title: command.Title,
            firstName: command.FirstName,
            lastName: command.LastName,
            middleName: command.MiddleName,
            titleEn: command.TitleEn,
            firstNameEn: command.FirstNameEn,
            lastNameEn: command.LastNameEn,
            middleNameEn: command.MiddleNameEn,
            address: command.Address);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        EmployeeDto dto = employee.ToDetailDto();
        employee.MapAuditableFieldsTo(ref dto);

        return dto;
    }
}