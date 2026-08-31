using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Converts text between common casing conventions.</summary>
public sealed partial class CaseTool : ITool
{
    public string Id => "case";
    public string Name => "Case Converter";
    public string Description => "Convert text to camelCase, snake_case, kebab-case and more.";
    public ToolCategory Category => ToolCategory.Text;

    public CaseResult Convert(string text)
    {
        var words = Tokenize(text);
        return new CaseResult(
            Lower: (text ?? string.Empty).ToLowerInvariant(),
            Upper: (text ?? string.Empty).ToUpperInvariant(),
            Title: string.Join(' ', words.Select(Capitalize)),
            Camel: ToCamel(words),
            Pascal: string.Concat(words.Select(Capitalize)),
            Snake: string.Join('_', words.Select(w => w.ToLowerInvariant())),
            Kebab: string.Join('-', words.Select(w => w.ToLowerInvariant())),
            Constant: string.Join('_', words.Select(w => w.ToUpperInvariant())));
    }

    private static IReadOnlyList<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];
        // Split camelCase boundaries, then break on non-alphanumeric separators.
        var spaced = CamelBoundary().Replace(text, "$1 $2");
        return Separators().Split(spaced).Where(w => w.Length > 0).ToArray();
    }

    private static string ToCamel(IReadOnlyList<string> words)
    {
        if (words.Count == 0) return string.Empty;
        var sb = new StringBuilder(words[0].ToLowerInvariant());
        for (var i = 1; i < words.Count; i++)
            sb.Append(Capitalize(words[i]));
        return sb.ToString();
    }

    private static string Capitalize(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();

    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex CamelBoundary();

    [GeneratedRegex(@"[^A-Za-z0-9]+")]
    private static partial Regex Separators();
}

/// <summary>Text rendered in several casing styles.</summary>
public sealed record CaseResult(
    string Lower,
    string Upper,
    string Title,
    string Camel,
    string Pascal,
    string Snake,
    string Kebab,
    string Constant);
