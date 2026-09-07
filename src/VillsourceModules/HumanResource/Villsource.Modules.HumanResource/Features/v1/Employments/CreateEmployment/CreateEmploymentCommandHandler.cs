using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Employments.CreateEmployment;

public class CreateEmploymentCommandHandler(HumanResourceDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<CreateEmploymentCommand, EmploymentDto>
{
    public async ValueTask<EmploymentDto> Handle(CreateEmploymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Employee employee = await dbContext.Employees
                                .Where(e => e.Ref == command.EmployeeRef)
                                .SingleOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Employee with Ref: {command.EmployeeRef} not found");

        if (!EmploymentType.TryGet(command.Type, out EmploymentType? employmentType))
            throw new CustomException($"Employment type {command.Type} is define.", [], HttpStatusCode.BadRequest);

        EmployeeEmploymentAtTimeSpec spec = new(employmentType, employee.Id, timeProvider.GetUtcNow());
        bool isEmployed = await dbContext.Employments
            .ApplySpecification(spec)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (isEmployed)
            throw new CustomException($"Employee '{command.EmployeeRef}' is already employed with '{command.Type}' ",
                [], HttpStatusCode.Conflict);

        Employment employment = Employment.Create(
            employmentId: employee.Id,
            effectiveDate: command.EffectiveDate,
            type: employmentType);
        
        employment.SetNote(command.Note);
        dbContext.Employments.Add(employment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        EmploymentDto dto = employment.ToDto();
        employment.MapAuditableFieldsTo(ref dto);
        return dto;
    }
}