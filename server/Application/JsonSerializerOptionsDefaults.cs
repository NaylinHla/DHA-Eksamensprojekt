using System.Text.Encodings.Web;
using System.Text.Json;

namespace Application;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions CaseInsensitive = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static readonly JsonSerializerOptions CamelCase = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions WriteIndented = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static readonly JsonSerializerOptions MqttSerialize = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}