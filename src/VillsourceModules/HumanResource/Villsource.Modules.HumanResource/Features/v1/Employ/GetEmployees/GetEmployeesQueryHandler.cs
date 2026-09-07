using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Employ.GetEmployees;

public class GetEmployeesQueryHandler(HumanResourceDbContext dbContext) : IQueryHandler<GetEmployeesQuery, ICollection<EmployeeBriefDto>>
{
    public async ValueTask<ICollection<EmployeeBriefDto>> Handle(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var dbQuery = dbContext.Employees;
        var employee = await dbQuery.ProjectToBriefDto().ToListAsync(cancellationToken).ConfigureAwait(false);
        return employee;
    }
}