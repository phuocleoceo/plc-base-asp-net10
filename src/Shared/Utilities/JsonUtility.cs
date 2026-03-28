using System.Text.Json;

namespace PlcBase.Shared.Utilities;

public static class JsonUtility
{
    // private static readonly JsonSerializerOptions DefaultOptions = new()
    //     {
    //         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //         PropertyNameCaseInsensitive = true,
    //     };

    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerOptions.Web);

    private static readonly JsonSerializerOptions IndentedOptions = new(JsonSerializerOptions.Web)
    {
        WriteIndented = true,
    };

    public static string Serialize(object obj, bool indented = false)
    {
        if (obj == null)
        {
            return "";
        }

        JsonSerializerOptions options = indented ? IndentedOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, options);
    }

    public static T Deserialize<T>(string objString)
    {
        if (string.IsNullOrWhiteSpace(objString))
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(objString, DefaultOptions);
    }
}
