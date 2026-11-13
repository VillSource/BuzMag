using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities;

internal static class JurisdictionRules
{
    private static readonly Regex ThTaxIdRegex = new(@"^\d{13}$", RegexOptions.Compiled);
    private static readonly Regex ThBranchCodeRegex = new(@"^\d{5}$", RegexOptions.Compiled);
    private static readonly Regex SgUenRegex = new(@"^[A-Z0-9]{9,10}$", RegexOptions.Compiled);
    private static readonly Regex UsEinRegex = new(@"^\d{2}-?\d{7}$", RegexOptions.Compiled);
    private static readonly Regex JpCorpNoRegex = new(@"^\d{13}$", RegexOptions.Compiled);
    private static readonly Regex VnTinRegex = new(@"^\d{10}(-\d{3})?$", RegexOptions.Compiled);

    private static readonly Regex PhTinRegex = new(@"^(\d{9}|\d{12}|\d{3}-\d{3}-\d{3}(-\d{3,5})?)$",
        RegexOptions.Compiled);

    private static readonly Regex PhBranchRegex = new(@"^\d{3,5}$", RegexOptions.Compiled);

    private static readonly Regex GenericTaxIdRegex = new(@"^[A-Z0-9\-/\. ]{4,30}$", RegexOptions.Compiled);
    private static readonly Regex GenericBranchCodeRegex = new(@"^[A-Z0-9\-_]{1,20}$", RegexOptions.Compiled);

    public static string GetDefaultHeadOfficeBranchCode(string countryCode) => countryCode switch
    {
        "TH" => "00000",
        "PH" => "000",
        _ => "HQ"
    };

    public static string NormalizeAndValidateTaxId(string countryCode, string taxId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taxId);
        var normalized = taxId.Trim().ToUpperInvariant();

        var isValid = countryCode switch
        {
            "TH" => ThTaxIdRegex.IsMatch(normalized),
            "SG" => SgUenRegex.IsMatch(normalized),
            "US" => UsEinRegex.IsMatch(normalized),
            "JP" => JpCorpNoRegex.IsMatch(normalized),
            "VN" => VnTinRegex.IsMatch(normalized),
            "PH" => PhTinRegex.IsMatch(normalized),
            _ => GenericTaxIdRegex.IsMatch(normalized)
        };

        if (!isValid)
        {
            throw new ArgumentException($"Invalid Tax ID format '{normalized}' for country '{countryCode}'.",
                nameof(taxId));
        }

        return normalized;
    }

    public static (string NormalizedCode, bool ResolvedIsHeadOffice) NormalizeAndValidateBranch(
        string countryCode,
        string branchCode,
        bool isHeadOffice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(branchCode);
        var normalized = branchCode.Trim().ToUpperInvariant();

        switch (countryCode)
        {
            case "TH":
                if (!ThBranchCodeRegex.IsMatch(normalized))
                {
                    throw new ArgumentException(
                        "Thailand tax branch code must be a 5-digit numeric string (e.g., 00000).", nameof(branchCode));
                }

                if (normalized == "00000")
                {
                    return (normalized, true);
                }

                if (isHeadOffice)
                {
                    throw new ArgumentException("Thailand head office branch code must be '00000'.",
                        nameof(branchCode));
                }

                return (normalized, false);

            case "PH":
                if (!PhBranchRegex.IsMatch(normalized))
                {
                    throw new ArgumentException("Philippines BIR branch code must be 3 to 5 digits (e.g., 000).",
                        nameof(branchCode));
                }

                return (normalized, isHeadOffice || normalized is "000" or "00000");

            default:
                if (!GenericBranchCodeRegex.IsMatch(normalized))
                {
                    throw new ArgumentException(
                        $"Branch code '{normalized}' must be 1-20 uppercase alphanumeric characters, hyphens, or underscores.",
                        nameof(branchCode));
                }

                return (normalized, isHeadOffice);
        }
    }
}