using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{

    public sealed class CreateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<CreateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(
            CreateJobGradeCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var normalizedCode = JobGradeRules.NormalizeAndValidateCode(request.Code);

            if (await dbContext.JobGrades.AnyAsync(g => g.Code == normalizedCode, cancellationToken))
            {
                throw new ConflictException($"Job grade code '{normalizedCode}' already exists.");
            }

            var entity = JobGrade.Create(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                rankOrder: request.RankOrder,
                description: request.Description,
                isActive: request.IsActive);

            await dbContext.JobGrades.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

}