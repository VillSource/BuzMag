using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Positions;

public class CreatePositionAssignmentCommandHandler(HumanResourceDbContext dbContext)
    : ICommandHandler<CreatePositionAssignmentCommand, PositionAssignmentDto>
{
    public async ValueTask<PositionAssignmentDto> Handle(CreatePositionAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.EmployeeRef == command.ManagerRef)
            throw new CustomException($"Circular Reporting line.", [], HttpStatusCode.BadRequest);

        if (!PositionTier.TryGet(command.PositionTier, out PositionTier? tier))
            throw new CustomException($"Position tier {command.PositionTier} is not defined.", [],
                HttpStatusCode.BadRequest);

        if (!PositionAssignmentType.TryGet(command.Type, out PositionAssignmentType? assignmentType))
            throw new CustomException($"Position assignment type {command.Type} is invalid.", [],
                HttpStatusCode.BadRequest);

        if (!(assignmentType == PositionAssignmentType.Initial ||
            assignmentType == PositionAssignmentType.Secondment ||
            assignmentType == PositionAssignmentType.Acting))
            throw new CustomException($"Assignment type '{command.Type}' is not for create.", [],
                HttpStatusCode.BadRequest);

        Employee employee = await dbContext.Employees
                                .Where(e => e.Ref == command.EmployeeRef)
                                .SingleOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Employee with Ref: {command.EmployeeRef} not found");

        PositionAssignment assignment = PositionAssignment.Create(
            employeeId: employee.Id,
            type: assignmentType,
            organizationUnitRef: command.OrganizationUnitRef,
            positionRef: command.PositionRef,
            positionTier: tier,
            isPrimary: command.IsPrimary,
            effectiveDate: command.EffectiveDate);

        if (!string.IsNullOrWhiteSpace(command.ManagerRef))
        {
            Employee manager = await dbContext.Employees
                                   .Where(e => e.Ref == command.ManagerRef)
                                   .SingleOrDefaultAsync(cancellationToken)
                                   .ConfigureAwait(false)
                               ?? throw new NotFoundException($"Manager with Ref: {command.ManagerRef} not found");

            assignment.AssignManager(manager);
        }

        dbContext.PositionAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        PositionAssignmentDto dto = assignment.ToDto();
        assignment.MapAuditableFieldsTo(ref dto);
        return dto;
    }
}