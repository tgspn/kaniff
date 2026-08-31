using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class UrlViewModel : ToolPageViewModel
{
    private readonly UrlEncodeTool _tool = new();

    public UrlViewModel() : base("URL Encode", "Percent-encode or decode text for URLs.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Output { get; set; } = string.Empty;

    [RelayCommand]
    private void Encode() => Output = _tool.Encode(Input);

    [RelayCommand]
    private void Decode() => Output = _tool.Decode(Input);
}
