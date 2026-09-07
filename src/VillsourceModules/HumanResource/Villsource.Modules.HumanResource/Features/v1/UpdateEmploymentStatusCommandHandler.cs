using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using FSH.Framework.Persistence.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1;

public class UpdateEmploymentStatusCommandHandler(HumanResourceDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<UpdateEmploymentStatusCommand, EmploymentDto>
{
    public async ValueTask<EmploymentDto> Handle(UpdateEmploymentStatusCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!EmploymentType.TryGet(command.Type, out EmploymentType? employmentType))
            throw new CustomException($"Employment type {command.Type} is not define.", [], HttpStatusCode.BadRequest);

        if (!EmploymentStatus.TryGet(command.Status, out EmploymentStatus? employmentStatus))
            throw new CustomException($"Employment status {command.Status} is not define.", [],
                HttpStatusCode.BadRequest);

        Employee employee = await dbContext.Employees
                                .Where(e => e.Ref == command.EmployeeRef)
                                .SingleOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Employee with Ref: {command.EmployeeRef} not found");

        DateTimeOffset now = timeProvider.GetUtcNow();
        EmployeeEmploymentAtTimeSpec spec = new(employmentType, employee.Id, now);
        var employments = await dbContext.Employments
                                .ApplySpecification(spec)
                                .AsTracking()
                                .Take(2)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false)
                            ?? throw new CustomException(
                                $"[invalid data for employment in database] Employment with type '{command.Type}' of employee '{command.EmployeeRef}' not found or found multiple employments.");
        
        Employment oldEmployment = employments.Count switch
        {
            0 => throw new CustomException($"Employment with type '{command.Type}' for employee '{command.EmployeeRef}' was not found.",[], HttpStatusCode.BadRequest),
            > 1 => throw new CustomException($"Invalid database state: Found multiple active employments with type '{command.Type}' for employee '{command.EmployeeRef}'."),
            _ => employments[0]
        };

        if (oldEmployment.Status == employmentStatus)
            throw new CustomException($"Employment already with status '{oldEmployment.Status}'.");

        oldEmployment.EffectiveTo = command.EffectiveDate - TimeSpan.FromDays(1);

        Employment newEmployment = Employment.Create(
            employmentId: employee.Id,
            effectiveDate: command.EffectiveDate,
            type: employmentType);
        newEmployment.SetStatus(employmentStatus);

        newEmployment.SetNote(command.Note);
        dbContext.Employments.Add(newEmployment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        EmploymentDto dto = newEmployment.ToDto();
        newEmployment.MapAuditableFieldsTo(ref dto);
        return dto;
    }
}

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

public sealed class EmployeeEmploymentAtTimeSpec : Specification<Employment>
{
    public EmployeeEmploymentAtTimeSpec(EmploymentType type, Guid employeeId, DateTimeOffset now)
    {
        Where(e => e.Type == type);
        Where(e => e.EmployeeId == employeeId);
        Where(e => e.EffectiveFrom <= now && (now <= e.EffectiveTo || e.EffectiveTo == null));
    }
}