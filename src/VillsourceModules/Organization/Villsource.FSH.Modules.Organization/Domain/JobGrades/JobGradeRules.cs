using System.Text.RegularExpressions;

// =============================================================================
// 1. DOMAIN LAYER (Rules, Aggregate Root, Domain Events)
// =============================================================================
namespace Villsource.FSH.Modules.Organization.Domain.JobGrades
{
    internal static partial class JobGradeRules
    {
        public const int MaxCodeLength = 20;
        public const int MaxNameLength = 150;
        public const int MaxDescriptionLength = 500;
        public const int MaxCategoryLength = 30;
        public const int MinRankOrder = 1;

        [GeneratedRegex(@"^[A-Z0-9\-_]{1,20}$", RegexOptions.CultureInvariant)]
        private static partial Regex CodeRegex();

        public static bool IsValidCode(string? code) =>
            !string.IsNullOrWhiteSpace(code) && CodeRegex().IsMatch(code.Trim().ToUpperInvariant());

        public static string NormalizeAndValidateCode(string code)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            var normalized = code.Trim().ToUpperInvariant();

            if (!CodeRegex().IsMatch(normalized))
            {
                throw new ArgumentException(
                    $"Job grade code '{normalized}' must be 1-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                    nameof(code));
            }

            return normalized;
        }

        public static (string Code, string Name, string InternationalName, string? Description) NormalizeAndValidate(
            string code,
            string name,
            string internationalName,
            int rankOrder,
            string? description)
        {
            var normalizedCode = NormalizeAndValidateCode(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

            if (rankOrder < MinRankOrder)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rankOrder),
                    $"RankOrder must be greater than or equal to {MinRankOrder}.");
            }

            var normalizedDescription = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();

            return (normalizedCode, name.Trim(), internationalName.Trim(), normalizedDescription);
        }
    }
}
