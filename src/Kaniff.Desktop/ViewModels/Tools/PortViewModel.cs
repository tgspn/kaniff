using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class PortViewModel : ToolPageViewModel
{
    private readonly PortTool _tool = new();

    public PortViewModel()
        : base("Port Check", "Test whether a TCP port is open — what telnet host port is used for.")
    {
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    public partial string Host { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    public partial string Port { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Detail { get; set; } = string.Empty;

    /// <summary>True once a check has produced a verdict, so the view can reveal the result panel.</summary>
    [ObservableProperty]
    public partial bool HasResult { get; set; }

    /// <summary>Drives the result colour: green when open, red otherwise.</summary>
    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    private bool CanCheck =>
        !IsBusy && !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Port);

    [RelayCommand(CanExecute = nameof(CanCheck))]
    private async Task CheckAsync()
    {
        if (!int.TryParse(Port.Trim(), out var port))
        {
            HasResult = true;
            IsOpen = false;
            Status = "Invalid port";
            Detail = $"'{Port}' is not a number.";
            return;
        }

        IsBusy = true;
        HasResult = false;

        try
        {
            var result = await _tool.CheckAsync(Host, port);
            var service = result.ServiceName is null ? string.Empty : $" ({result.ServiceName})";

            IsOpen = result.IsOpen;
            Status = result.Status switch
            {
                PortStatus.Open => $"Open{service}",
                PortStatus.Closed => $"Closed{service}",
                PortStatus.TimedOut => $"Timed out{service}",
                _ => "Unreachable",
            };
            Detail = $"{result.Message} ({result.ElapsedMilliseconds} ms)";
            HasResult = true;
        }
        catch (ArgumentException ex)
        {
            IsOpen = false;
            // ArgumentException.Message appends "(Parameter 'x')", which is
            // internal detail that means nothing to someone using the app.
            Detail = ex.Message.Split(" (Parameter")[0];
            Status = "Invalid input";
            HasResult = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
