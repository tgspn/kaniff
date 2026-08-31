using System.Text.RegularExpressions;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Tests a regular expression against an input and reports the matches.</summary>
public sealed class RegexTool : ITool
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    public string Id => "regex";
    public string Name => "Regex Tester";
    public string Description => "Test a regular expression and inspect matches and groups.";
    public ToolCategory Category => ToolCategory.Text;

    public RegexResult Match(string pattern, string input, bool ignoreCase = false, bool multiline = false)
    {
        var options = RegexOptions.None;
        if (ignoreCase) options |= RegexOptions.IgnoreCase;
        if (multiline) options |= RegexOptions.Multiline;

        var regex = new Regex(pattern ?? string.Empty, options, Timeout);
        var matches = new List<RegexMatchInfo>();
        foreach (Match m in regex.Matches(input ?? string.Empty))
        {
            var groups = new List<RegexGroupInfo>();
            foreach (Group g in m.Groups)
            {
                if (g.Success)
                    groups.Add(new RegexGroupInfo(g.Name, g.Value, g.Index));
            }
            matches.Add(new RegexMatchInfo(m.Value, m.Index, m.Length, groups));
        }
        return new RegexResult(matches);
    }
}

/// <summary>All matches produced by a regex run.</summary>
public sealed record RegexResult(IReadOnlyList<RegexMatchInfo> Matches);

/// <summary>A single regex match.</summary>
public sealed record RegexMatchInfo(string Value, int Index, int Length, IReadOnlyList<RegexGroupInfo> Groups);

/// <summary>A captured group within a match.</summary>
public sealed record RegexGroupInfo(string Name, string Value, int Index);
