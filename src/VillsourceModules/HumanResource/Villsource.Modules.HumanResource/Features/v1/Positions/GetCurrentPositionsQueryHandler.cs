using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Positions;

public class
    GetCurrentPositionsQueryHandler(HumanResourceDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetCurrentPositionsQuery, ICollection<PositionAssignmentDto>>
{
    public async ValueTask<ICollection<PositionAssignmentDto>> Handle(GetCurrentPositionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var employee = await dbContext.Employees.Where(e => e.Ref == query.EmployeeRef)
                           .SingleOrDefaultAsync(cancellationToken)
                           .ConfigureAwait(false)
                       ?? throw new NotFoundException($"Employee {query.EmployeeRef} not found");

        var spec = new PositionAssignmentAtTimeSpec(employee.Id, timeProvider.GetUtcNow(), query.IsPrimary);
        var currentPositions = await dbContext.PositionAssignments
            .ApplySpecification(spec)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return currentPositions.ToDto();
    }
}