using FSH.Framework.Core.Domain;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Villsource.FSH.Modules.Organization.Domain.Events;

public sealed record OrganizationCreatedDomainEvent(

    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed class OrganizationCreatedDomainEventHandlers(ILogger<OrganizationCreatedDomainEventHandlers> logger) :
    INotificationHandler<OrganizationCreatedDomainEvent>
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
}