using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class ColorViewModel : ToolPageViewModel
{
    private readonly ColorTool _tool = new();

    public ColorViewModel() : base("Color", "Convert colors between HEX, RGB and HSL.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = "#3498db";

    [ObservableProperty]
    public partial string Hex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Rgb { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Hsl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IBrush Swatch { get; set; } = Brushes.Transparent;

    [ObservableProperty]
    public partial string Error { get; set; } = string.Empty;

    [RelayCommand]
    private void Convert()
    {
        try
        {
            var r = _tool.Convert(Input);
            Hex = r.Hex;
            Rgb = r.Rgb;
            Hsl = r.Hsl;
            Swatch = new SolidColorBrush(Color.Parse(r.Hex));
            Error = string.Empty;
        }
        catch (Exception ex)
        {
            Error = $"⚠ {ex.Message}";
        }
    }
}
