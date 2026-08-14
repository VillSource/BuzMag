using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Villsource.Tool.UniqueKey;

public sealed class VillsourceIdGenerator : ValueGenerator<string>
{
    public override string Next(EntityEntry entry)
    {
        return VillsourceId.NextKey();
    }

    public override bool GeneratesTemporaryValues { get { return false; } }
}