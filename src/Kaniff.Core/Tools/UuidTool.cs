using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Generates random UUIDs (version 4).</summary>
public sealed class UuidTool : ITool
{
    public string Id => "uuid";
    public string Name => "UUID Generator";
    public string Description => "Generate one or more random version-4 UUIDs.";
    public ToolCategory Category => ToolCategory.Text;

    public IReadOnlyList<string> Generate(int count = 1, bool uppercase = false)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");

        var result = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var value = Guid.NewGuid().ToString();
            result.Add(uppercase ? value.ToUpperInvariant() : value);
        }
        return result;
    }
}
