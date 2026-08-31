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
    public partial string PublicIp { get; set; } = "…";

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

            try
            {
                var result = await _tool.GetPublicIpAsync();
                PublicIp = result.Ip;
                PublicSource = $"via {result.Source}";
            }
            catch (Exception ex)
            {
                PublicIp = "unavailable";
                PublicSource = ex.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
