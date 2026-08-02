using Mediator;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Events;

public sealed class OrganizationStructureEventHandlers(ILogger<OrganizationStructureEventHandlers> logger)
    : INotificationHandler<OrganizationCreatedDomainEvent>
{
    public ValueTask Handle(OrganizationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("");
        }

        return default;
    }
}