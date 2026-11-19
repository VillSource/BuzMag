using System.Text;
using Elsa.Workflows;
using Elsa.Workflows.Models;

namespace Villsource.Modules.Elsa.Core.Modifiers;

/// <summary>
/// Populates missing output descriptions (such as the inherited <c>Result</c> output on
/// <see cref="CodeActivity{T}"/>) with the C# return type name so workflow authors know which variable type to create.
/// </summary>
public sealed class TypedOutputDescriptionModifier : IActivityDescriptorModifier
{
    public void Modify(ActivityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        foreach (OutputDescriptor output in descriptor.Outputs.Where(static o => string.IsNullOrWhiteSpace(o.Description)))
        {
            output.Description = $"Returns {FormatTypeName(output.Type)}.";
        }
    }

    internal static string FormatTypeName(Type type)
    {
        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying is not null)
        {
            return $"{FormatTypeName(nullableUnderlying)}?";
        }

        if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            return elementType is null ? type.Name : $"{FormatTypeName(elementType)}[]";
        }

        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(object)) return "object";

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string baseName = type.Name;
        int tickIndex = baseName.IndexOf('`', StringComparison.Ordinal);
        if (tickIndex > 0)
        {
            baseName = baseName[..tickIndex];
        }

        Type[] genericArgs = type.GetGenericArguments();
        StringBuilder sb = new(baseName);
        sb.Append('<');
        for (int i = 0; i < genericArgs.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(FormatTypeName(genericArgs[i]));
        }

        sb.Append('>');
        return sb.ToString();
    }
}
