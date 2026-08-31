using System.Text.Json;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Shared System.Text.Json options for the JSON tooling.</summary>
internal static class JsonToolOptions
{
    public static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true
    };

    public static readonly JsonDocumentOptions Lenient = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };
}

/// <summary>Formats, minifies and validates JSON.</summary>
public sealed class JsonTool : ITool
{
    public string Id => "json";
    public string Name => "JSON Format/Minify";
    public string Description => "Pretty-print, minify and validate JSON documents.";
    public ToolCategory Category => ToolCategory.Json;

    public string Format(string json)
    {
        using var doc = JsonDocument.Parse(json, JsonToolOptions.Lenient);
        return JsonSerializer.Serialize(doc.RootElement, JsonToolOptions.Pretty);
    }

    public string Minify(string json)
    {
        using var doc = JsonDocument.Parse(json, JsonToolOptions.Lenient);
        return JsonSerializer.Serialize(doc.RootElement);
    }

    /// <summary>Returns null when the JSON is valid, otherwise a human-readable error.</summary>
    public string? Validate(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json, JsonToolOptions.Lenient);
            return null;
        }
        catch (JsonException ex)
        {
            return ex.Message;
        }
    }
}
