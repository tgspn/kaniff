using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Discovers the machine's public IP (via online services) and local IP addresses.</summary>
public sealed class IpTool : ITool
{
    private static readonly string[] PublicIpEndpoints =
    [
        "https://ifconfig.me/ip",
        "https://api.ipify.org",
        "https://icanhazip.com",
        "https://ifconfig.co/ip"
    ];

    private readonly HttpClient _http;

    public IpTool(HttpClient? httpClient = null)
    {
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            _http.DefaultRequestHeaders.Add("User-Agent", "curl/8.0 (Kaniff)");
    }

    public string Id => "ip";
    public string Name => "My IP";
    public string Description => "Show your public IP (ifconfig.me with fallbacks) and local addresses.";
    public ToolCategory Category => ToolCategory.Network;

    /// <summary>Queries public IP services in order and returns the first successful result.</summary>
    public async Task<PublicIpResult> GetPublicIpAsync(CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        foreach (var endpoint in PublicIpEndpoints)
        {
            try
            {
                var response = await _http.GetStringAsync(endpoint, cancellationToken).ConfigureAwait(false);
                var ip = response.Trim();
                if (IPAddress.TryParse(ip, out _))
                    return new PublicIpResult(ip, endpoint);
                errors.Add($"{endpoint}: unexpected response '{Truncate(ip)}'");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                errors.Add($"{endpoint}: {ex.Message}");
            }
        }

        throw new InvalidOperationException(
            "Could not determine public IP. Tried:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    /// <summary>Enumerates local IPv4/IPv6 addresses from active network interfaces.</summary>
    public IReadOnlyList<LocalAddress> GetLocalAddresses(bool includeIPv6 = true)
    {
        var result = new List<LocalAddress>();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up ||
                nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            foreach (var ip in nic.GetIPProperties().UnicastAddresses)
            {
                var family = ip.Address.AddressFamily;
                if (family == AddressFamily.InterNetwork ||
                    (includeIPv6 && family == AddressFamily.InterNetworkV6 && !ip.Address.IsIPv6LinkLocal))
                {
                    result.Add(new LocalAddress(nic.Name, ip.Address.ToString(), family == AddressFamily.InterNetwork));
                }
            }
        }
        return result;
    }

    private static string Truncate(string value) =>
        value.Length <= 60 ? value : value[..60] + "...";
}

/// <summary>Public IP address plus the service that reported it.</summary>
public sealed record PublicIpResult(string Ip, string Source);

/// <summary>A local IP address bound to a network interface.</summary>
public sealed record LocalAddress(string InterfaceName, string Address, bool IsIPv4);
