using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries
{
    public sealed class GetJobGradeByRefIdQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobGradeByRefIdQuery, JobGradeDetailDto>
    {
        public async ValueTask<JobGradeDetailDto> Handle(
            GetJobGradeByRefIdQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .AsNoTracking()
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            return entity.ToDetailDto();
        }
    }

}