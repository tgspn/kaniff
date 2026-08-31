namespace Kaniff.Core.Abstractions;

/// <summary>
/// Metadata contract every tool exposes so the CLI and desktop app can discover
/// and list it. The actual operations live as strongly-typed methods on each tool.
/// </summary>
public interface ITool
{
    /// <summary>Stable identifier used as the CLI verb (e.g. "base64").</summary>
    string Id { get; }

    /// <summary>Human-friendly name shown in the UI.</summary>
    string Name { get; }

    /// <summary>Short description of what the tool does.</summary>
    string Description { get; }

    /// <summary>Group the tool belongs to.</summary>
    ToolCategory Category { get; }
}
