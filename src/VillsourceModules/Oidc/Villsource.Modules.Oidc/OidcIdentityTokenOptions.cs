using System.ComponentModel.DataAnnotations;

namespace Villsource.Modules.Oidc;

internal sealed class OidcIdentityTokenOptions : IValidatableObject
{
    public const string SectionName = "JwtOptions";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            yield return new ValidationResult($"{SectionName}:Issuer is required.", [nameof(Issuer)]);
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            yield return new ValidationResult($"{SectionName}:Audience is required.", [nameof(Audience)]);
        }

        if (string.IsNullOrEmpty(SigningKey))
        {
            yield return new ValidationResult($"{SectionName}:SigningKey is required.", [nameof(SigningKey)]);
        }
        else
        {
            if (SigningKey.Length < 32)
            {
                yield return new ValidationResult($"{SectionName}:SigningKey must be at least 32 characters long.",
                    [nameof(SigningKey)]);
            }

            if (!SigningKey.All(character => character <= 0x7f))
            {
                yield return new ValidationResult($"{SectionName}:SigningKey must only contain ASCII characters.",
                    [nameof(SigningKey)]);
            }

            if (SigningKey.Contains("replace-with", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult($"{SectionName}:SigningKey cannot be a sample placeholder.",
                    [nameof(SigningKey)]);
            }
        }

        if (AccessTokenMinutes <= 0)
        {
            yield return new ValidationResult($"{SectionName}:AccessTokenMinutes must be greater than zero.",
                [nameof(AccessTokenMinutes)]);
        }

        if (RefreshTokenDays <= 0)
        {
            yield return new ValidationResult($"{SectionName}:RefreshTokenDays must be greater than zero.",
                [nameof(RefreshTokenDays)]);
        }
    }
}