using FSH.Framework.Eventing.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Events;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;
using Villsource.FSH.Modules.Organization.Domain.Positions.Events;

namespace Villsource.FSH.Modules.Organization.Events;

public sealed class OrganizationIntegrationEventPublishers(
    IEventBus eventBus,
    ILogger<OrganizationIntegrationEventPublishers> logger) :
    INotificationHandler<PositionDeactivatedDomainEvent>,
    INotificationHandler<PositionDelegationRevokedDomainEvent>,
    INotificationHandler<OrganizationUnitMovedDomainEvent>,
    INotificationHandler<LegalEntityDeletedDomainEvent>
{
    public async ValueTask Handle(PositionDeactivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Publishing PositionDeactivatedIntegrationEvent for Position {PositionId}", notification.PositionId);
        }

        var integrationEvent = new PositionDeactivatedIntegrationEvent(
            Id: notification.EventId,
            OccurredOnUtc: notification.OccurredOnUtc.UtcDateTime,
            TenantId: notification.TenantId,
            CorrelationId: notification.EventId.ToString(),
            Source: nameof(OrganizationIntegrationEventPublishers),
            PositionId: notification.PositionId,
            LegalEntityId: notification.LegalEntityId,
            Code: notification.Code);

        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask Handle(PositionDelegationRevokedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Publishing PositionDelegationRevokedIntegrationEvent for Delegation {PositionDelegationId}", notification.PositionDelegationId);
        }

        var integrationEvent = new PositionDelegationRevokedIntegrationEvent(
            Id: notification.EventId,
            OccurredOnUtc: notification.OccurredOnUtc.UtcDateTime,
            TenantId: notification.TenantId,
            CorrelationId: notification.EventId.ToString(),
            Source: nameof(OrganizationIntegrationEventPublishers),
            PositionDelegationId: notification.PositionDelegationId,
            LegalEntityId: notification.LegalEntityId,
            DelegatorPositionId: notification.DelegatorPositionId,
            DelegatePositionId: notification.DelegatePositionId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask Handle(OrganizationUnitMovedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Publishing OrganizationUnitMovedIntegrationEvent for OrganizationUnit {OrganizationUnitId}", notification.OrganizationUnitId);
        }

        var integrationEvent = new OrganizationUnitMovedIntegrationEvent(
            Id: notification.EventId,
            OccurredOnUtc: notification.OccurredOnUtc.UtcDateTime,
            TenantId: notification.TenantId,
            CorrelationId: notification.EventId.ToString(),
            Source: nameof(OrganizationIntegrationEventPublishers),
            OrganizationUnitId: notification.OrganizationUnitId,
            OldParentId: notification.OldParentId,
            NewParentId: notification.NewParentId,
            NewHierarchyPath: notification.NewHierarchyPath,
            NewLevel: notification.NewLevel);

        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask Handle(LegalEntityDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Publishing LegalEntityDeletedIntegrationEvent for LegalEntity {LegalEntityId}", notification.LegalEntityId);
        }

        var integrationEvent = new LegalEntityDeletedIntegrationEvent(
            Id: notification.EventId,
            OccurredOnUtc: notification.OccurredOnUtc.UtcDateTime,
            TenantId: notification.TenantId,
            CorrelationId: notification.EventId.ToString(),
            Source: nameof(OrganizationIntegrationEventPublishers),
            LegalEntityId: notification.LegalEntityId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }
}
