using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class CaseViewModel : ToolPageViewModel
{
    private readonly CaseTool _tool = new();

    public CaseViewModel() : base("Case", "Convert text between casing conventions.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Result { get; set; } = string.Empty;

    [RelayCommand]
    private void Convert()
    {
        var r = _tool.Convert(Input);
        Result = string.Join(Environment.NewLine,
            $"lower      : {r.Lower}",
            $"UPPER      : {r.Upper}",
            $"Title      : {r.Title}",
            $"camelCase  : {r.Camel}",
            $"PascalCase : {r.Pascal}",
            $"snake_case : {r.Snake}",
            $"kebab-case : {r.Kebab}",
            $"CONSTANT   : {r.Constant}");
    }
}
