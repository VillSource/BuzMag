using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Villsource.Modules.Oidc.Data;

public sealed class OidcDbInitializer(
    OidcDbContext dbContext,
    ILogger<OidcDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Oidc] applied migrations");
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[Oidc] seeded default organization");
    }
}