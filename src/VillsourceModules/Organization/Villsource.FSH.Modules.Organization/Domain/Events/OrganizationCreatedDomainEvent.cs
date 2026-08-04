using FSH.Framework.Core.Domain;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Domain.Events;

public sealed record OrganizationUnitDeletedDomainEvent(
    Guid OrganizationId,
    Guid OrganizationUnitId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record OrganizationCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed class OrganizationCreatedDomainEventHandlers(
    ILogger<OrganizationCreatedDomainEventHandlers> logger,
    OrganizationDbContext dbContext) :
    INotificationHandler<OrganizationCreatedDomainEvent>,
    INotificationHandler<OrganizationUnitDeletedDomainEvent>
{
    public ValueTask Handle(OrganizationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Handling ProductCreatedDomainEvent for ProductId:");
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