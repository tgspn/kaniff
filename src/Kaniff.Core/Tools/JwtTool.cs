using System.Text;
using System.Text.Json;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Decodes JWT tokens (header and payload) without verifying the signature.</summary>
public sealed class JwtTool : ITool
{
    public string Id => "jwt";
    public string Name => "JWT Decoder";
    public string Description => "Inspect a JWT's header and payload (no signature verification).";
    public ToolCategory Category => ToolCategory.Security;

    public JwtDecodeResult Decode(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new FormatException("Token is empty.");

        var parts = token.Trim().Split('.');
        if (parts.Length < 2)
            throw new FormatException("A JWT must have at least a header and a payload separated by '.'.");

        var headerJson = PrettyJson(DecodeSegment(parts[0]));
        var payloadRaw = DecodeSegment(parts[1]);
        var payloadJson = PrettyJson(payloadRaw);
        var signature = parts.Length > 2 ? parts[2] : string.Empty;

        DateTimeOffset? exp = null, iat = null, nbf = null;
        try
        {
            using var doc = JsonDocument.Parse(payloadRaw);
            var root = doc.RootElement;
            exp = ReadUnixTime(root, "exp");
            iat = ReadUnixTime(root, "iat");
            nbf = ReadUnixTime(root, "nbf");
        }
        catch (JsonException)
        {
            // Payload is not valid JSON; timestamps stay null.
        }

        return new JwtDecodeResult(headerJson, payloadJson, signature, exp, iat, nbf);
    }

    private static string DecodeSegment(string segment)
    {
        var normalized = segment.Replace('-', '+').Replace('_', '/');
        var padding = normalized.Length % 4;
        if (padding > 0)
            normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }

    private static DateTimeOffset? ReadUnixTime(JsonElement root, string name)
    {
        if (root.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt64(out var seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }
        return null;
    }

    private static string PrettyJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, JsonToolOptions.Pretty);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}

/// <summary>Result of decoding a JWT.</summary>
public sealed record JwtDecodeResult(
    string HeaderJson,
    string PayloadJson,
    string Signature,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? NotBefore)
{
    public bool IsExpired => ExpiresAt is { } exp && exp < DateTimeOffset.UtcNow;
}
