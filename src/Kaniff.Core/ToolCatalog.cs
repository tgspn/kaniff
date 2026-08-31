using Kaniff.Core.Abstractions;
using Kaniff.Core.Tools;

namespace Kaniff.Core;

/// <summary>
/// Central registry of the available tools. Add a new tool here to make it
/// discoverable by both the CLI and the desktop app.
/// </summary>
public static class ToolCatalog
{
    public static IReadOnlyList<ITool> All { get; } =
    [
        new IpTool(),
        new Base64Tool(),
        new UrlEncodeTool(),
        new JwtTool(),
        new HashTool(),
        new UuidTool(),
        new TimestampTool(),
        new CaseTool(),
        new ColorTool(),
        new RegexTool(),
        new QrTool(),
        new StringCompareTool(),
        new JsonTool()
    ];
}
