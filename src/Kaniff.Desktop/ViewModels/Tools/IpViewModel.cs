using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class IpViewModel : ToolPageViewModel
{
    private readonly IpTool _tool = new();

    public IpViewModel() : base("My IP", "Discover your public and local IP addresses.")
    {
        _ = RefreshAsync();
    }

    [ObservableProperty]
    public partial string PublicIpV4 { get; set; } = "…";

    [ObservableProperty]
    public partial string PublicIpV6 { get; set; } = "…";

    [ObservableProperty]
    public partial string PublicSource { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<string> LocalAddresses { get; } = [];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            LocalAddresses.Clear();
            foreach (var addr in _tool.GetLocalAddresses())
                LocalAddresses.Add($"{addr.Address}  ({(addr.IsIPv4 ? "IPv4" : "IPv6")}, {addr.InterfaceName})");

            PublicIpV4 = "…";
            PublicIpV6 = "…";
            PublicSource = string.Empty;

            try
            {
                var pair = await _tool.GetPublicIpsAsync();

                // "not available" is a normal answer here: a network without
                // IPv6 has no public v6 address, and vice versa.
                PublicIpV4 = pair.V4?.Ip ?? "not available";
                PublicIpV6 = pair.V6?.Ip ?? "not available";

                var source = pair.V4?.Source ?? pair.V6?.Source;
                PublicSource = pair.IsEmpty
                    ? "No public address could be resolved. Are you offline?"
                    : $"via {source}";
            }
            catch (Exception ex)
            {
                PublicIpV4 = "unavailable";
                PublicIpV6 = "unavailable";
                PublicSource = ex.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
