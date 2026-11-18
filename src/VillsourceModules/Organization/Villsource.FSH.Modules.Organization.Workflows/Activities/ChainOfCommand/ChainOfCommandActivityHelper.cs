using System.Text.Json;
using System.Text.Json.Serialization;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

internal static class ChainOfCommandActivityHelper
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

    public static ChainOfCommandPositionDto? ParsePosition(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is ChainOfCommandPositionDto dto)
        {
            return dto;
        }

        if (value is string json)
        {
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<ChainOfCommandPositionDto>(json, JsonOptions);
        }

        if (value is JsonElement element)
        {
            if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return null;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                string? inner = element.GetString();
                return string.IsNullOrWhiteSpace(inner)
                    ? null
                    : JsonSerializer.Deserialize<ChainOfCommandPositionDto>(inner, JsonOptions);
            }

            return element.Deserialize<ChainOfCommandPositionDto>(JsonOptions);
        }

        string serialized = JsonSerializer.Serialize(value, JsonOptions);
        return JsonSerializer.Deserialize<ChainOfCommandPositionDto>(serialized, JsonOptions);
    }
}
