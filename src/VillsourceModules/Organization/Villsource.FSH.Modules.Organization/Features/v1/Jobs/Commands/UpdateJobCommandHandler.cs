using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class UpdateJobCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdateJobCommand, string>
{
    public async ValueTask<string> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = JobRules.NormalizeAndValidateCode(request.Code);

        // Single DB round-trip: load tracked Job + scalar subqueries for duplicate Code and target JobGrade
        var target = await dbContext.Jobs
                         .Where(j => j.ReferenceId == request.RefId)
                         .Select(j => new
                         {
                             Entity = j,
                             CodeExists = j.Code != normalizedCode &&
                                          dbContext.Jobs.Any(other => other.Code == normalizedCode && other.Id != j.Id),
                             GradeId = dbContext.JobGrades
                                 .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                                 .Select(g => (Guid?)g.Id)
                                 .FirstOrDefault(),
                             GradeIsActive = dbContext.JobGrades
                                 .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                                 .Select(g => (bool?)g.IsActive)
                                 .FirstOrDefault()
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

        if (target.CodeExists)
        {
            throw new ConflictException($"Job code '{normalizedCode}' already exists.");
        }

        if (!target.GradeId.HasValue)
        {
            throw new NotFoundException($"Job grade '{request.DefaultJobGradeRefId}' was not found.");
        }

        if (target.GradeIsActive != true)
        {
            throw new InvalidOperationException($"Job grade '{request.DefaultJobGradeRefId}' is inactive.");
        }

        var changed = target.Entity.Update(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            defaultJobGradeId: target.GradeId.Value,
            jobFamily: request.JobFamily,
            description: request.Description);

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return target.Entity.ReferenceId;
    }
}