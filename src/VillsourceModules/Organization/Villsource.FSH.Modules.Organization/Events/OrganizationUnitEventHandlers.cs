using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Events;

public sealed class OrganizationUnitEventHandlers(
    ILogger<OrganizationUnitEventHandlers> logger,
    OrganizationDbContext dbContext) :
    INotificationHandler<OrganizationCreatedDomainEvent>,
    INotificationHandler<OrganizationUnitCreatedDomainEvent>,
    INotificationHandler<OrganizationUnitDeletedDomainEvent>
{
    public ValueTask Handle(OrganizationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PlaceHolder for OrganizationCreatedDomainEvent Handler");
        }

        return default;
    }

    public ValueTask Handle(OrganizationUnitCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PlaceHolder for OrganizationUnitCreatedDomainEvent Handler");
        }

        return default;
    }

    public async ValueTask Handle(OrganizationUnitDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var deleted = await dbContext.OrganizationUnits
                          .IgnoreQueryFilters([QueryFilters.SoftDelete])
                          .Where(x => x.OrganizationId == notification.OrganizationId)
                          .Where(x => x.Id == notification.OrganizationUnitId)
                          .FirstOrDefaultAsync(cancellationToken)
                          .ConfigureAwait(false) ??
                      throw new NotFoundException("Deleted OrganizationUnit not found");

        var descendants = await deleted.GetDescendantsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        foreach (var descendant in descendants)
        {
            descendant.Delete();
            descendant.ClearDomainEvents();
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}