using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Events;

public sealed class OrganizationUnitEventHandlers(
    ILogger<OrganizationUnitEventHandlers> logger) :
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

    public ValueTask Handle(OrganizationUnitDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PlaceHolder for OrganizationUnitDeletedDomainEvent Handler");
        }

        return default;
    }
}