using FSH.Framework.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations;

internal static class DocumentTypeScopeValidator
{
    public static async Task EnsureRegisteredAsync(
        OrganizationDbContext dbContext,
        DelegationType delegationType,
        string? documentTypeScope,
        CancellationToken cancellationToken)
    {
        if (!delegationType.RequiresDocumentScope)
        {
            return;
        }

        var normalized = PositionDelegationRules.NormalizeDocumentScope(delegationType, documentTypeScope);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var codes = normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var registeredCodes = await dbContext.DocumentTypes
            .AsNoTracking()
            .Where(d => codes.Contains(d.Code))
            .Select(d => d.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var missing = codes.Except(registeredCodes, StringComparer.OrdinalIgnoreCase).ToArray();
        if (missing.Length > 0)
        {
            throw new NotFoundException(
                $"Unregistered document type code(s): {string.Join(", ", missing)}.");
        }
    }
}
