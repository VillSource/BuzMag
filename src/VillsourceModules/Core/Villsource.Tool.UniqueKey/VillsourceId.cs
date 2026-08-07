using FluentValidation;
using FluentValidation.Validators;
using NanoidDotNet;
using System.Text.RegularExpressions;

namespace Villsource.Tool.UniqueKey;

public static partial class VillsourceId
{
    public const int KeySizes = 11;
    public static string Key { get { return NextKey(); } }

    public static string NextKey()
    {
        return Nanoid.Generate(Nanoid.Alphabets.LettersAndDigits, KeySizes);
    }

    public static bool IsKeyValidate(ReadOnlySpan<char> key)
    {
        return IdRegex().IsMatch(key);
    }

    public static IRuleBuilderOptions<T, string?> IsVillsourceId<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        ArgumentNullException.ThrowIfNull(ruleBuilder);
        return ruleBuilder.SetValidator(new VillsourceIdValidator<T>());
    }

    public static void ThrowIfInvalid(ReadOnlySpan<char> key)
    {
        if (!IsKeyValidate(key))
            throw new ArgumentException($"Invalid format key: {key}", nameof(key));
    }

    [GeneratedRegex(@"^[a-zA-Z0-9]{11}$", RegexOptions.Compiled)]
    private static partial Regex IdRegex();
}

public class VillsourceIdValidator<T> : PropertyValidator<T,string?>, IVillsourceIdValidator {

    public override string Name => "VillsourceIdValidator";

    public override bool IsValid(ValidationContext<T> context, string? value) {
        return VillsourceId.IsKeyValidate(value);
    }

    protected override string GetDefaultMessageTemplate(string errorCode) {
        return Localized(errorCode, Name);
    }
}

public interface IVillsourceIdValidator : IPropertyValidator {
}
