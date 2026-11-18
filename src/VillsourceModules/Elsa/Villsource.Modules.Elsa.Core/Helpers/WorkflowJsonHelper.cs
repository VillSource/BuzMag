using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Villsource.Modules.Elsa.Core.Helpers;

/// <summary>
/// Shared JSON serialization, parsing, and input normalization helpers for Elsa workflow activities.
/// </summary>
public static class WorkflowJsonHelper
{
    public static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string RequireNonEmpty(string? value, string inputName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Activity input '{inputName}' must not be null or empty.", inputName);
        }

        return value.Trim();
    }

    public static string SerializeJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public static string? SerializeNullableJson<T>(T? value)
        where T : class
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }

    public static JsonObject? ToJsonObject<T>(T? value)
        where T : class
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonObject jsonObject)
        {
            return jsonObject;
        }

        return JsonSerializer.SerializeToNode(value, JsonOptions) as JsonObject;
    }

    public static JsonArray ToJsonArray<T>(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return JsonSerializer.SerializeToNode(items, JsonOptions) as JsonArray ?? [];
    }

    public static JsonObject ToListResultJsonObject<T>(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new JsonObject
        {
            ["items"] = ToJsonArray(items),
            ["count"] = items.Count
        };
    }

    public static T? Parse<T>(object? value)
    {
        if (value is null)
        {
            return default;
        }

        if (value is T typed)
        {
            return typed;
        }

        if (value is JsonObject jsonObject)
        {
            return jsonObject.Deserialize<T>(JsonOptions);
        }

        if (value is JsonNode jsonNode)
        {
            return jsonNode.Deserialize<T>(JsonOptions);
        }

        if (value is string json)
        {
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        if (value is JsonElement element)
        {
            if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return default;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                string? inner = element.GetString();
                return string.IsNullOrWhiteSpace(inner)
                    ? default
                    : JsonSerializer.Deserialize<T>(inner, JsonOptions);
            }

            return element.Deserialize<T>(JsonOptions);
        }

        string serialized = JsonSerializer.Serialize(value, JsonOptions);
        return JsonSerializer.Deserialize<T>(serialized, JsonOptions);
    }
}
