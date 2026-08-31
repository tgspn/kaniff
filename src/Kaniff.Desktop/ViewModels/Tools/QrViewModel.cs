using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class QrViewModel : ToolPageViewModel
{
    private readonly QrTool _tool = new();

    public QrViewModel() : base("QR Code", "Generate a QR code from text.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Bitmap? Image { get; set; }

    [ObservableProperty]
    public partial string Error { get; set; } = string.Empty;

    [RelayCommand]
    private void Generate()
    {
        try
        {
            using var stream = new MemoryStream(_tool.GeneratePng(Input));
            Image = new Bitmap(stream);
            Error = string.Empty;
        }
        catch (Exception ex)
        {
            Image = null;
            Error = $"⚠ {ex.Message}";
        }
    }
}
