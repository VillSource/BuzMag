using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations;

public sealed class RevokePositionDelegationCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<RevokePositionDelegationCommand, string>
{
    public async ValueTask<string> Handle(
        RevokePositionDelegationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.PositionDelegations
                         .FirstOrDefaultAsync(d => d.ReferenceId == request.RefId, cancellationToken)
                     ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

        if (!entity.IsRevoked)
        {
            entity.Revoke(
                revokedBy: currentUser.GetUserId().ToString(),
                revocationReason: request.RevocationReason);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return entity.ReferenceId;
    }
}