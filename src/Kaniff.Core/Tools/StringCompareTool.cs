using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Compares two strings and reports how they differ.</summary>
public sealed class StringCompareTool : ITool
{
    public string Id => "strcmp";
    public string Name => "String Comparer";
    public string Description => "Compare two strings and locate the first difference.";
    public ToolCategory Category => ToolCategory.Text;

    public StringCompareResult Compare(string? left, string? right, bool ignoreCase = false, bool ignoreWhitespace = false)
    {
        var a = Normalize(left, ignoreWhitespace);
        var b = Normalize(right, ignoreWhitespace);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var areEqual = string.Equals(a, b, comparison);

        int? firstDiff = null;
        if (!areEqual)
        {
            var min = Math.Min(a.Length, b.Length);
            var i = 0;
            while (i < min && CharsMatch(a[i], b[i], ignoreCase))
                i++;
            firstDiff = i;
        }

        return new StringCompareResult(areEqual, firstDiff, a.Length, b.Length);
    }

    private static bool CharsMatch(char x, char y, bool ignoreCase) =>
        ignoreCase ? char.ToUpperInvariant(x) == char.ToUpperInvariant(y) : x == y;

    private static string Normalize(string? value, bool ignoreWhitespace)
    {
        value ??= string.Empty;
        return ignoreWhitespace
            ? string.Concat(value.Where(c => !char.IsWhiteSpace(c)))
            : value;
    }
}

/// <summary>Result of comparing two strings.</summary>
public sealed record StringCompareResult(
    bool AreEqual,
    int? FirstDifferenceIndex,
    int LeftLength,
    int RightLength);
