using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class IpViewModel : ToolPageViewModel
{
    /// <summary>Shown while a lookup is in flight.</summary>
    private const string Placeholder = "…";

    private readonly IpTool _tool = new();

    public IpViewModel() : base("My IP", "Discover your public and local IP addresses.")
    {
        _ = RefreshAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPublicIpV4))]
    public partial string PublicIpV4 { get; set; } = Placeholder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPublicIpV6))]
    public partial string PublicIpV6 { get; set; } = Placeholder;

    [ObservableProperty]
    public partial string PublicSource { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>True when <see cref="PublicIpV4"/> holds a real address rather than a status message.</summary>
    public bool HasPublicIpV4 => IsAddress(PublicIpV4);

    /// <summary>True when <see cref="PublicIpV6"/> holds a real address rather than a status message.</summary>
    public bool HasPublicIpV6 => IsAddress(PublicIpV6);

    public ObservableCollection<LocalAddressItem> LocalAddresses { get; } = [];

    // The address fields double as a status line ("…", "not available"), so parsing
    // is the honest way to tell an address from a message.
    private static bool IsAddress(string value) => IPAddress.TryParse(value, out _);

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            LocalAddresses.Clear();
            foreach (var addr in _tool.GetLocalAddresses())
            {
                LocalAddresses.Add(new LocalAddressItem(
                    addr.Address,
                    $"{addr.Address}  ({(addr.IsIPv4 ? "IPv4" : "IPv6")}, {addr.InterfaceName})"));
            }

            PublicIpV4 = Placeholder;
            PublicIpV6 = Placeholder;
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

/// <summary>
/// A local address in two forms: the bare value for the clipboard and an
/// annotated one for display.
/// </summary>
public sealed record LocalAddressItem(string Address, string Display);
