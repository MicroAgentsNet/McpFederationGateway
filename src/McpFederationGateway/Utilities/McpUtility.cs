using System.Text.Json;

namespace McpFederationGateway.Utilities;

/// <summary>
/// Utility methods for MCP argument parsing and conversion.
/// </summary>
public static class McpUtility
{
    /// <summary>
    /// Extracts a string value from an argument that could be either a string or a JsonElement.
    /// </summary>
    /// <param name="value">The value to extract from (string or JsonElement)</param>
    /// <returns>The string value, or null if extraction fails</returns>
    public static string? ExtractString(object? value)
    {
        return value switch
        {
            string s => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            _ => null
        };
    }

    /// <summary>
    /// Extracts a dictionary from an argument that could be either a dictionary or a JsonElement object.
    /// </summary>
    /// <param name="value">The value to extract from</param>
    /// <returns>A dictionary of arguments, or an empty dictionary if extraction fails</returns>
    public static IDictionary<string, object?> ExtractDictionary(object? value)
    {
        return value switch
        {
            IDictionary<string, object?> d => d,
            JsonElement je when je.ValueKind == JsonValueKind.Object =>
                je.EnumerateObject().ToDictionary(p => p.Name, p => (object?)p.Value),
            _ => new Dictionary<string, object?>()
        };
    }

    /// <summary>
    /// Tries to get a string value from a dictionary, handling both string and JsonElement types.
    /// </summary>
    /// <param name="arguments">The arguments dictionary</param>
    /// <param name="key">The key to look up</param>
    /// <param name="value">The extracted string value (output parameter)</param>
    /// <returns>True if the value was found and successfully extracted, false otherwise</returns>
    public static bool TryGetString(IDictionary<string, object?> arguments, string key, out string? value)
    {
        if (arguments.TryGetValue(key, out var obj))
        {
            value = ExtractString(obj);
            return value != null;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Tries to get a dictionary value from a dictionary, handling both dictionary and JsonElement types.
    /// </summary>
    /// <param name="arguments">The arguments dictionary</param>
    /// <param name="key">The key to look up</param>
    /// <param name="value">The extracted dictionary value (output parameter)</param>
    /// <returns>True if the value was found, false otherwise</returns>
    public static bool TryGetDictionary(IDictionary<string, object?> arguments, string key, out IDictionary<string, object?> value)
    {
        if (arguments.TryGetValue(key, out var obj))
        {
            value = ExtractDictionary(obj);
            return true;
        }
        value = new Dictionary<string, object?>();
        return false;
    }
}
