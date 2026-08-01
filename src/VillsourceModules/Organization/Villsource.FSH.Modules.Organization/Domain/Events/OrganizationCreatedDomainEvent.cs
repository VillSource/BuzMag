using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed record OrganizationCreatedDomainEvent(Guid Id, DateTimeOffset Ts) : DomainEvent(Id, Ts);