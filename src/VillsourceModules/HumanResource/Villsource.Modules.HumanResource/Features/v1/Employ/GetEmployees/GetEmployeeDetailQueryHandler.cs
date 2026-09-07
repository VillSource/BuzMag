using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Employ.GetEmployees;

public class GetEmployeeDetailQueryHandler(HumanResourceDbContext dbContext) : IQueryHandler<GetEmployeeDetailQuery, EmployeeDto>
{
    public async ValueTask<EmployeeDto> Handle(GetEmployeeDetailQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var spec = new EmployeeByRefSpec(query.EmployeeRef);
        var dbQuery = dbContext.Employees.ApplySpecification(spec);

        var employee = await dbQuery.ProjectToDetailDto().SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Employee not found for '{query.EmployeeRef}'");

        return employee;
    }
}