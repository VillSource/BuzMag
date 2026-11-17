using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class CreateJobCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreateJobCommand, string>
{
    public async ValueTask<string> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = JobRules.NormalizeAndValidateCode(request.Code);

        // Single DB round-trip: resolve DefaultJobGrade + check duplicate Code simultaneously
        var lookup = await dbContext.JobGrades
                         .AsNoTracking()
                         .Where(g => g.ReferenceId == request.DefaultJobGradeRefId)
                         .Select(g => new
                         {
                             g.Id,
                             g.IsActive,
                             CodeExists = dbContext.Jobs.Any(j => j.Code == normalizedCode)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Job grade '{request.DefaultJobGradeRefId}' was not found.");

        if (!lookup.IsActive)
        {
            throw new InvalidOperationException($"Job grade '{request.DefaultJobGradeRefId}' is inactive.");
        }

        if (lookup.CodeExists)
        {
            throw new ConflictException($"Job code '{normalizedCode}' already exists.");
        }

        var entity = Job.Create(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            defaultJobGradeId: lookup.Id,
            jobFamily: request.JobFamily,
            description: request.Description,
            isActive: request.IsActive);

        await dbContext.Jobs.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ReferenceId;
    }
}