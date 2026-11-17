using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class ActivatePositionCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<ActivatePositionCommand, string>
{
    public async ValueTask<string> Handle(ActivatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var position = await dbContext.Positions
                           .Include(p => p.ReportsToPosition)
                           .FirstOrDefaultAsync(p => p.ReferenceId == request.RefId, cancellationToken)
                       ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (!position.IsActive)
        {
            position.Activate();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return position.ReferenceId;
    }
}