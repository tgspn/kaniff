using System.Text;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Percent-encodes and decodes text for use in URLs.</summary>
public sealed class UrlEncodeTool : ITool
{
    public string Id => "url";
    public string Name => "URL Encode/Decode";
    public string Description => "Percent-encode or decode text for URLs.";
    public ToolCategory Category => ToolCategory.Encoding;

    public string Encode(string text) => Uri.EscapeDataString(text ?? string.Empty);

    public string Decode(string text) => Uri.UnescapeDataString(text ?? string.Empty);
}
