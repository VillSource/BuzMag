namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

internal static class PositionDelegationRules
{
    public const int MaxDelegationTypeLength = 50;
    public const int MaxDocumentTypeScopeLength = 500;
    public const int MaxReasonLength = 500;
    public const int MaxRevokedByLength = 100;

    public static (DateTimeOffset UtcFrom, DateTimeOffset UtcTo, string? NormalizedScope, string? NormalizedReason)
        NormalizeAndValidate(
            DelegationType delegationType,
            DateTimeOffset validFrom,
            DateTimeOffset validTo,
            string? documentTypeScope,
            string? reason)
    {
        ArgumentNullException.ThrowIfNull(delegationType);

        var utcFrom = validFrom.ToUniversalTime();
        var utcTo = validTo.ToUniversalTime();

        if (utcTo <= utcFrom)
        {
            throw new ArgumentException("ValidTo must be strictly later than ValidFrom.", nameof(validTo));
        }

        var normalizedScope = NormalizeDocumentScope(delegationType, documentTypeScope);
        var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        return (utcFrom, utcTo, normalizedScope, normalizedReason);
    }

    public static string? NormalizeDocumentScope(DelegationType delegationType, string? scope)
    {
        if (!delegationType.RequiresDocumentScope)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new ArgumentException(
                $"DocumentTypeScope is required when DelegationType is '{delegationType.Key}'.",
                nameof(scope));
        }

        var tokens = scope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (tokens.Length == 0)
        {
            throw new ArgumentException("At least one document type code must be provided.", nameof(scope));
        }

        var joined = string.Join(',', tokens);
        if (joined.Length > MaxDocumentTypeScopeLength)
        {
            throw new ArgumentException(
                $"Normalized DocumentTypeScope exceeds maximum length of {MaxDocumentTypeScopeLength} characters.",
                nameof(scope));
        }

        return joined;
    }
}