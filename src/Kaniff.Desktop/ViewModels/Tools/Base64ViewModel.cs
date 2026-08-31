using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class Base64ViewModel : ToolPageViewModel
{
    private readonly Base64Tool _tool = new();

    public Base64ViewModel() : base("Base64", "Encode and decode text to/from Base64.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Output { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool UrlSafe { get; set; }

    [RelayCommand]
    private void Encode() => Run(() => _tool.Encode(Input, UrlSafe));

    [RelayCommand]
    private void Decode() => Run(() => _tool.Decode(Input));

    private void Run(Func<string> action)
    {
        try
        {
            Output = action();
        }
        catch (Exception ex)
        {
            Output = $"⚠ {ex.Message}";
        }
    }
}
