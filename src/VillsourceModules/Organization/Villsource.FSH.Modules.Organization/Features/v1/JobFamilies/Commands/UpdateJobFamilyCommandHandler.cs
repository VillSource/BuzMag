using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class UpdateJobFamilyCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdateJobFamilyCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateJobFamilyCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.JobFamilies
            .FirstOrDefaultAsync(f => f.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Job family '{request.RefId}' was not found.");

        var normalizedCode = JobFamilyRules.NormalizeAndValidateCode(request.Code);

        if (await dbContext.JobFamilies
                .AnyAsync(f => f.Code == normalizedCode && f.Id != entity.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Job family code '{normalizedCode}' already exists.");
        }

        if (entity.Update(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                description: request.Description))
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return entity.ReferenceId;
    }
}
