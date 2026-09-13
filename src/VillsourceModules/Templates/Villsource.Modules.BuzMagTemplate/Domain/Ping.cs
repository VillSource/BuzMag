using FSH.Framework.Core.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.BuzMagTemplate.Domain;

public sealed partial  class Ping : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
}