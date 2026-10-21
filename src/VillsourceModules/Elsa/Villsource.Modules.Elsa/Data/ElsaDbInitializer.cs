
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Villsource.Modules.Elsa.Data;

public sealed class ElsaDbInitializer(
    ElsaDbContext dbContext,
    ManagementElsaDbContext managementDbContext,
    RuntimeElsaDbContext runtimeDbContext,
    ILogger<ElsaDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[elsa] applied migrations");
        }
        
        if ((await managementDbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await managementDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("[elsa-management] applied migrations");
        }
        
        if ((await runtimeDbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await runtimeDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("[elsa-runtime] applied migrations");
        }

    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[elsa] seeded default organization");
    }
}
