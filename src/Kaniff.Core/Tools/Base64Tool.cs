using System.Text;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Encodes and decodes text to/from Base64, with optional URL-safe alphabet.</summary>
public sealed class Base64Tool : ITool
{
    public string Id => "base64";
    public string Name => "Base64 Encode/Decode";
    public string Description => "Convert text to and from Base64 (standard or URL-safe).";
    public ToolCategory Category => ToolCategory.Encoding;

    public string Encode(string text, bool urlSafe = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(text ?? string.Empty);
        var base64 = Convert.ToBase64String(bytes);
        return urlSafe ? ToUrlSafe(base64) : base64;
    }

    public string Decode(string base64, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = DecodeToBytes(base64);
        return encoding.GetString(bytes);
    }

    /// <summary>Decodes to raw bytes, tolerating both standard and URL-safe input.</summary>
    public byte[] DecodeToBytes(string base64)
    {
        var normalized = FromUrlSafe((base64 ?? string.Empty).Trim());
        var padding = normalized.Length % 4;
        if (padding > 0)
            normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
        return Convert.FromBase64String(normalized);
    }

    private static string ToUrlSafe(string base64) =>
        base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string FromUrlSafe(string base64) =>
        base64.Replace('-', '+').Replace('_', '/');
}
