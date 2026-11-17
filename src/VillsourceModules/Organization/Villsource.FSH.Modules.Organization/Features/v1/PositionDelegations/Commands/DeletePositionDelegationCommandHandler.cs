using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations
{
    public sealed class DeletePositionDelegationCommandHandler(
        OrganizationDbContext dbContext,
        ICurrentUser currentUser)
        : IRequestHandler<DeletePositionDelegationCommand, string>
    {
        public async ValueTask<string> Handle(
            DeletePositionDelegationCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.PositionDelegations
                             .FirstOrDefaultAsync(d => d.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

            entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ReferenceId;
        }
    }

}