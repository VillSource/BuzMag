using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Villsource.Modules.BuzMagTemplate.Data;

public sealed class BuzMagTemplateDbInitializer(
    BuzMagTemplateDbContext dbContext,
    ILogger<BuzMagTemplateDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[BuzMagTemplate] applied migrations");
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[BuzMagTemplate] seeded default organization");
    }
}
