using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class RegexViewModel : ToolPageViewModel
{
    private readonly RegexTool _tool = new();

    public RegexViewModel() : base("Regex", "Test a regular expression and inspect matches.") { }

    [ObservableProperty]
    public partial string Pattern { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IgnoreCase { get; set; }

    [ObservableProperty]
    public partial bool Multiline { get; set; }

    [ObservableProperty]
    public partial string Result { get; set; } = string.Empty;

    [RelayCommand]
    private void Run()
    {
        try
        {
            var r = _tool.Match(Pattern, Input, IgnoreCase, Multiline);
            if (r.Matches.Count == 0)
            {
                Result = "no matches";
                return;
            }
            var sb = new StringBuilder($"{r.Matches.Count} match(es):\n");
            foreach (var m in r.Matches)
            {
                sb.AppendLine($"  [{m.Index}] '{m.Value}'");
                foreach (var g in m.Groups.Skip(1))
                    sb.AppendLine($"      group {g.Name}: '{g.Value}' @ {g.Index}");
            }
            Result = sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            Result = $"⚠ {ex.Message}";
        }
    }
}
