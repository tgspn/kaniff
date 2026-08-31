using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class TimestampViewModel : ToolPageViewModel
{
    private readonly TimestampTool _tool = new();

    public TimestampViewModel() : base("Timestamp", "Convert Unix time to a date and back.")
    {
        Now();
    }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Result { get; set; } = string.Empty;

    [RelayCommand]
    private void Convert()
    {
        try
        {
            var r = string.IsNullOrWhiteSpace(Input)
                ? _tool.Now()
                : long.TryParse(Input.Trim(), out var unix) ? _tool.FromUnix(unix) : _tool.FromDate(Input.Trim());
            Show(r);
        }
        catch (Exception ex)
        {
            Result = $"⚠ {ex.Message}";
        }
    }

    [RelayCommand]
    private void Now()
    {
        var r = _tool.Now();
        Input = r.UnixSeconds.ToString();
        Show(r);
    }

    private void Show(TimestampResult r) =>
        Result = $"ISO 8601 : {r.Iso8601}\nLocal    : {r.Local}\nUnix (s) : {r.UnixSeconds}\nUnix (ms): {r.UnixMilliseconds}";
}
