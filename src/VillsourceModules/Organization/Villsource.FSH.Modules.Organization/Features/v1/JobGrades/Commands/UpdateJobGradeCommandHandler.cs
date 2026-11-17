using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class UpdateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<UpdateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(
            UpdateJobGradeCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            var normalizedCode = JobGradeRules.NormalizeAndValidateCode(request.Code);

            if (!string.Equals(entity.Code, normalizedCode, StringComparison.Ordinal) &&
                await dbContext.JobGrades.AnyAsync(g => g.Code == normalizedCode && g.Id != entity.Id, cancellationToken))
            {
                throw new ConflictException($"Job grade code '{normalizedCode}' already exists.");
            }

            var changed = entity.Update(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                rankOrder: request.RankOrder,
                description: request.Description);

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }

}