using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class CreateJobFamilyCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreateJobFamilyCommand, string>
{
    public async ValueTask<string> Handle(
        CreateJobFamilyCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = JobFamilyRules.NormalizeAndValidateCode(request.Code);

        if (await dbContext.JobFamilies
                .AnyAsync(f => f.Code == normalizedCode, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Job family code '{normalizedCode}' already exists.");
        }

        var entity = JobFamily.Create(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            description: request.Description,
            isActive: request.IsActive);

        await dbContext.JobFamilies.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.ReferenceId;
    }
}
