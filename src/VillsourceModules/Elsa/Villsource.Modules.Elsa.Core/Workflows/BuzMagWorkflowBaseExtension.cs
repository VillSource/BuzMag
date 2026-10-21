using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json;
using JsonException = System.Text.Json.JsonException;

namespace Villsource.Modules.Elsa.Core.Workflows;

public static partial class BuzMagWorkflowBaseExtension
{
    private static readonly JsonSerializerOptions WorkflowInputJsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    public static T? GetWfInput<T>(this ActivityExecutionContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);
        var logger = context.GetRequiredService<ILogger<BuzMagWorkflowBase>>();

        IDictionary<string, object> inputs = context.WorkflowExecutionContext.Input;

        if (!inputs.TryGetValue(key, out object? value))
        {
            LogWorkflowInputNotFoundKey(logger, key);
            return default;
        }

        return value switch
        {
            T typedValue => typedValue,
            JsonElement jsonElement => jsonElement.ConvertTo<T>(context),
            _ => value.ConvertFallback<T>(context)
        };
    }

    private static T? ConvertTo<T>(this JsonElement jsonElement, ActivityExecutionContext context)
    {
        var logger = context.GetRequiredService<ILogger<BuzMagWorkflowBase>>();
        try
        {
            return jsonElement.Deserialize<T>(WorkflowInputJsonSerializerOptions);
        }
        catch (Exception e) when (e is JsonException or JsonSerializationException)
        {
            LogCanNotDeserializeJsonToTypeType(logger, jsonElement, typeof(T).Name);
            return default;
        }
    }

    private static T? ConvertFallback<T>(this object fallbackValue, ActivityExecutionContext context)
    {
        var logger = context.GetRequiredService<ILogger<BuzMagWorkflowBase>>();

        try
        {
            Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T?)Convert.ChangeType(fallbackValue, t, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            LogCannotConvertValueToType(logger, fallbackValue.ToString(), typeof(T).Name);
            return default;
        }
    }

    [LoggerMessage(LogLevel.Information, "Workflow input not found: {key}")]
    static partial void LogWorkflowInputNotFoundKey(this ILogger<BuzMagWorkflowBase> logger, string key);

    [LoggerMessage(LogLevel.Information, "Can not deserialize {JsonElement} to type {Type}")]
    static partial void LogCanNotDeserializeJsonToTypeType(this ILogger<BuzMagWorkflowBase> logger,
        JsonElement jsonElement, string type);

    [LoggerMessage(LogLevel.Information, "Cannot convert {Value} to {Type}")]
    static partial void
        LogCannotConvertValueToType(this ILogger<BuzMagWorkflowBase> logger, string? value, string type);
}