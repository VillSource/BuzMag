using System.Globalization;
using System.Text.Json;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.Modules.Elsa.Core.Activities.OptionsProviders;
using Villsource.Modules.Elsa.Core.Constants;

namespace Villsource.Modules.Elsa.Core.Activities;

/// <summary>
/// Compares two input values using the selected comparison operator and outputs a boolean result.
/// </summary>
[Activity(
    Category = CoreActivityConstants.Category,
    DisplayName = "Compare Values",
    Description = "Compares two input values using the selected operator and outputs the boolean result."
)]
public class CompareValues : CodeActivity<bool>
{
    [Input(
        DisplayName = "Left",
        Description = "The first value to compare.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<object?> Left { get; set; } = default!;

    [Input(
        DisplayName = "Operator",
        Description = "Select the comparison operator.",
        UIHint = InputUIHints.DropDown,
        UIHandler = typeof(ComparisonOperatorOptionsProvider),
        DefaultValue = "=="
    )]
    public Input<string> Operator { get; set; } = new("==");

    [Input(
        DisplayName = "Right",
        Description = "The second value to compare.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<object?> Right { get; set; } = default!;

    protected override void Execute(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        object? left = Left.GetOrDefault(context);
        string? op = Operator.GetOrDefault(context);
        object? right = Right.GetOrDefault(context);

        bool result = Evaluate(left, op, right);
        context.SetResult(result);
    }

    internal static bool Evaluate(object? leftValue, string? operatorValue, object? rightValue)
    {
        if (string.IsNullOrWhiteSpace(operatorValue))
        {
            return false;
        }

        string op = operatorValue.Trim();
        if (op is not ("==" or "!=" or ">" or "<" or ">=" or "<="))
        {
            return false;
        }

        object? left = UnwrapValue(leftValue);
        object? right = UnwrapValue(rightValue);

        if (left is null || right is null)
        {
            return op switch
            {
                "==" => left is null && right is null,
                "!=" => left is not null || right is not null,
                _ => false
            };
        }

        if (TryCompareNumeric(left, right, op, out bool numericResult))
        {
            return numericResult;
        }

        if (TryCompareBoolean(left, right, op, out bool boolResult))
        {
            return boolResult;
        }

        if (TryCompareDateTimeOffset(left, right, op, out bool dateResult))
        {
            return dateResult;
        }

        if (TryCompareEnum(left, right, op, out bool enumResult))
        {
            return enumResult;
        }

        if (TryCompareGuid(left, right, op, out bool guidResult))
        {
            return guidResult;
        }

        if (left is string leftString && right is string rightString)
        {
            return CompareStrings(leftString, rightString, op);
        }

        if (left.GetType() == right.GetType() && left is IComparable comparable)
        {
            return ApplyComparison(comparable.CompareTo(right), op);
        }

        return op switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            _ => false
        };
    }

    private static object? UnwrapValue(object? value)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.TryGetDecimal(out decimal dec) ? dec : element.GetDouble(),
            JsonValueKind.String => element.GetString(),
            _ => element.GetRawText()
        };
    }

    private static bool TryCompareNumeric(object left, object right, string op, out bool result)
    {
        result = false;

        bool leftIsNumeric = IsNumericType(left);
        bool rightIsNumeric = IsNumericType(right);

        if (!leftIsNumeric && !rightIsNumeric)
        {
            // When both inputs are numeric strings and an ordering operator is used, compare numerically.
            if (op is ">" or "<" or ">=" or "<=" &&
                left is string leftStr &&
                right is string rightStr &&
                TryParseDecimal(leftStr, out decimal lDec) &&
                TryParseDecimal(rightStr, out decimal rDec))
            {
                result = ApplyComparison(decimal.Compare(lDec, rDec), op);
                return true;
            }

            return false;
        }

        if (TryToDecimal(left, out decimal leftDecimal) && TryToDecimal(right, out decimal rightDecimal))
        {
            result = ApplyComparison(decimal.Compare(leftDecimal, rightDecimal), op);
            return true;
        }

        if (TryToDouble(left, out double leftDouble) && TryToDouble(right, out double rightDouble))
        {
            if (double.IsNaN(leftDouble) || double.IsNaN(rightDouble))
            {
                result = op == "!=";
                return true;
            }

            result = ApplyComparison(leftDouble.CompareTo(rightDouble), op);
            return true;
        }

        result = op == "!=";
        return true;
    }

    private static bool TryCompareBoolean(object left, object right, string op, out bool result)
    {
        result = false;

        if (left is bool leftBool && right is bool rightBool)
        {
            result = op switch
            {
                "==" => leftBool == rightBool,
                "!=" => leftBool != rightBool,
                _ => false
            };
            return true;
        }

        if (left is bool lBool && right is string rStr && bool.TryParse(rStr.Trim(), out bool parsedRight))
        {
            result = op switch
            {
                "==" => lBool == parsedRight,
                "!=" => lBool != parsedRight,
                _ => false
            };
            return true;
        }

        if (right is bool rBool && left is string lStr && bool.TryParse(lStr.Trim(), out bool parsedLeft))
        {
            result = op switch
            {
                "==" => parsedLeft == rBool,
                "!=" => parsedLeft != rBool,
                _ => false
            };
            return true;
        }

        return false;
    }

    private static bool TryCompareDateTimeOffset(object left, object right, string op, out bool result)
    {
        result = false;

        bool leftIsDate = left is DateTimeOffset or DateTime;
        bool rightIsDate = right is DateTimeOffset or DateTime;
        if (!leftIsDate && !rightIsDate)
        {
            return false;
        }

        if (TryToDateTimeOffset(left, out DateTimeOffset leftDate) &&
            TryToDateTimeOffset(right, out DateTimeOffset rightDate))
        {
            result = ApplyComparison(DateTimeOffset.Compare(leftDate, rightDate), op);
            return true;
        }

        result = op == "!=";
        return true;
    }

    private static bool TryCompareEnum(object left, object right, string op, out bool result)
    {
        result = false;

        if (left is not Enum && right is not Enum)
        {
            return false;
        }

        if (left is Enum leftEnum && right is Enum rightEnum && leftEnum.GetType() == rightEnum.GetType())
        {
            result = ApplyEnumComparison(leftEnum, rightEnum, op);
            return true;
        }

        if (left is Enum lEnum && TryConvertToEnum(lEnum.GetType(), right, out object? parsedRight))
        {
            result = ApplyEnumComparison(lEnum, (Enum)parsedRight!, op);
            return true;
        }

        if (right is Enum rEnum && TryConvertToEnum(rEnum.GetType(), left, out object? parsedLeft))
        {
            result = ApplyEnumComparison((Enum)parsedLeft!, rEnum, op);
            return true;
        }

        result = op == "!=";
        return true;
    }

    private static bool ApplyEnumComparison(Enum left, Enum right, string op)
    {
        return op switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            _ => ApplyComparison(left.CompareTo(right), op)
        };
    }

    private static bool TryCompareGuid(object left, object right, string op, out bool result)
    {
        result = false;

        if (left is not Guid && right is not Guid)
        {
            return false;
        }

        if (TryToGuid(left, out Guid leftGuid) && TryToGuid(right, out Guid rightGuid))
        {
            result = ApplyComparison(leftGuid.CompareTo(rightGuid), op);
            return true;
        }

        result = op == "!=";
        return true;
    }

    private static bool CompareStrings(string left, string right, string op)
    {
        return op switch
        {
            "==" => string.Equals(left, right, StringComparison.Ordinal),
            "!=" => !string.Equals(left, right, StringComparison.Ordinal),
            _ => ApplyComparison(string.Compare(left, right, StringComparison.Ordinal), op)
        };
    }

    private static bool ApplyComparison(int comparison, string op)
    {
        return op switch
        {
            "==" => comparison == 0,
            "!=" => comparison != 0,
            ">" => comparison > 0,
            "<" => comparison < 0,
            ">=" => comparison >= 0,
            "<=" => comparison <= 0,
            _ => false
        };
    }

    private static bool IsNumericType(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryToDecimal(object value, out decimal result)
    {
        switch (value)
        {
            case decimal d:
                result = d;
                return true;
            case string s:
                return TryParseDecimal(s, out result);
            case float f when !float.IsNaN(f) && !float.IsInfinity(f):
                return TryConvertFloatToDecimal(f, out result);
            case double dbl when !double.IsNaN(dbl) && !double.IsInfinity(dbl):
                return TryConvertDoubleToDecimal(dbl, out result);
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return true;
            default:
                result = 0m;
                return false;
        }
    }

    private static bool TryConvertFloatToDecimal(float value, out decimal result)
    {
        if (value is < (float)decimal.MinValue or > (float)decimal.MaxValue)
        {
            result = 0m;
            return false;
        }

        result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryConvertDoubleToDecimal(double value, out decimal result)
    {
        if (value is < (double)decimal.MinValue or > (double)decimal.MaxValue)
        {
            result = 0m;
            return false;
        }

        result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryToDouble(object value, out double result)
    {
        if (value is string s)
        {
            return double.TryParse(
                s.Trim(),
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out result);
        }

        if (IsNumericType(value))
        {
            result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return true;
        }

        result = 0d;
        return false;
    }

    private static bool TryToDateTimeOffset(object value, out DateTimeOffset result)
    {
        switch (value)
        {
            case DateTimeOffset dto:
                result = dto;
                return true;
            case DateTime dt:
                DateTime utcOrUnspecified = dt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                    : dt;
                result = new DateTimeOffset(utcOrUnspecified);
                return true;
            case string s:
                return DateTimeOffset.TryParse(
                    s.Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out result);
            default:
                result = default;
                return false;
        }
    }

    private static bool TryConvertToEnum(Type enumType, object candidate, out object? parsed)
    {
        if (candidate is string s)
        {
            return Enum.TryParse(enumType, s.Trim(), ignoreCase: true, out parsed);
        }

        if (IsNumericType(candidate))
        {
            try
            {
                Type underlying = Enum.GetUnderlyingType(enumType);
                object converted = Convert.ChangeType(candidate, underlying, CultureInfo.InvariantCulture);
                parsed = Enum.ToObject(enumType, converted);
                return true;
            }
            catch (Exception ex) when (ex is InvalidCastException or OverflowException)
            {
                parsed = null;
                return false;
            }
        }

        parsed = null;
        return false;
    }

    private static bool TryToGuid(object value, out Guid result)
    {
        switch (value)
        {
            case Guid guid:
                result = guid;
                return true;
            case string s:
                return Guid.TryParse(s.Trim(), out result);
            default:
                result = Guid.Empty;
                return false;
        }
    }
}
