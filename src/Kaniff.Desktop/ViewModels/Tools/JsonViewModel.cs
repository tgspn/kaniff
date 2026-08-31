using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class JsonViewModel : ToolPageViewModel
{
    private readonly JsonTool _tool = new();

    public JsonViewModel() : base("JSON", "Format, minify and validate JSON.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Output { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;

    [RelayCommand]
    private void Format() => Run(() => _tool.Format(Input));

    [RelayCommand]
    private void Minify() => Run(() => _tool.Minify(Input));

    [RelayCommand]
    private void Validate()
    {
        var error = _tool.Validate(Input);
        Status = error is null ? "✔ Valid JSON" : $"⚠ {error}";
    }

    private void Run(Func<string> action)
    {
        try
        {
            Output = action();
            Status = "✔ OK";
        }
        catch (Exception ex)
        {
            Status = $"⚠ {ex.Message}";
        }
    }
}
