using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Discovers the machine's public IP (via online services) and local IP addresses.</summary>
public sealed class IpTool : ITool, IDisposable
{
    private static readonly string[] PublicIpEndpoints =
    [
        "https://ifconfig.me/ip",
        "https://api.ipify.org",
        "https://icanhazip.com",
        "https://ifconfig.co/ip"
    ];

    // Hostnames that only resolve to A records, so the request cannot go out
    // over IPv6 no matter how the machine is configured.
    private static readonly string[] IPv4Endpoints =
    [
        "https://api.ipify.org",
        "https://ipv4.icanhazip.com"
    ];

    // AAAA-only hostnames, the mirror image of the list above.
    private static readonly string[] IPv6Endpoints =
    [
        "https://api6.ipify.org",
        "https://ipv6.icanhazip.com"
    ];

    private readonly HttpClient _http;
    private readonly bool _ownsClient;

    public IpTool(HttpClient? httpClient = null)
    {
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        _ownsClient = httpClient is null;
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            _http.DefaultRequestHeaders.Add("User-Agent", "curl/8.0 (Kaniff)");
    }

    public string Id => "ip";
    public string Name => "My IP";
    public string Description => "Show your public IP (ifconfig.me with fallbacks) and local addresses.";
    public ToolCategory Category => ToolCategory.Network;

    /// <summary>Queries public IP services in order and returns the first successful result.</summary>
    /// <remarks>
    /// On a dual-stack machine the address family you get back is whatever the
    /// operating system picked for the connection, which is usually IPv6. Use
    /// <see cref="GetPublicIpsAsync"/> when you want both families.
    /// </remarks>
    public async Task<PublicIpResult> GetPublicIpAsync(CancellationToken cancellationToken = default)
    {
        var (result, errors) = await TryEndpointsAsync(_http, PublicIpEndpoints, cancellationToken).ConfigureAwait(false);
        return result ?? throw new InvalidOperationException(
            "Could not determine public IP. Tried:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    /// <summary>
    /// Looks up the public IPv4 and IPv6 addresses independently, so a dual-stack
    /// machine reports both instead of only the family the OS happened to use.
    /// </summary>
    /// <remarks>
    /// Either half may be null: an IPv4-only network has no public IPv6, and an
    /// IPv6-only network has no public IPv4. Both lookups run concurrently, and a
    /// failure on one side never hides a success on the other.
    /// </remarks>
    public async Task<PublicIpPair> GetPublicIpsAsync(CancellationToken cancellationToken = default)
    {
        var v4Task = QueryFamilyAsync(IPv4Endpoints, AddressFamily.InterNetwork, cancellationToken);
        var v6Task = QueryFamilyAsync(IPv6Endpoints, AddressFamily.InterNetworkV6, cancellationToken);
        await Task.WhenAll(v4Task, v6Task).ConfigureAwait(false);
        return new PublicIpPair(await v4Task.ConfigureAwait(false), await v6Task.ConfigureAwait(false));
    }

    /// <summary>
    /// Queries <paramref name="endpoints"/> over a client pinned to
    /// <paramref name="family"/>. Returns null when the machine has no address in
    /// that family, which is an expected outcome rather than an error.
    /// </summary>
    private async Task<PublicIpResult?> QueryFamilyAsync(
        string[] endpoints, AddressFamily family, CancellationToken cancellationToken)
    {
        // A dedicated handler per family: even though the hostnames are A-only or
        // AAAA-only, pinning the socket guarantees the request cannot escape over
        // the other family via a CNAME or a DNS64/NAT64 translation.
        using var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, token) =>
            {
                var socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, token).ConfigureAwait(false);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };

        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
        client.DefaultRequestHeaders.Add("User-Agent", "curl/8.0 (Kaniff)");

        var (result, _) = await TryEndpointsAsync(client, endpoints, cancellationToken).ConfigureAwait(false);

        // Guard against a service echoing the wrong family back at us.
        if (result is not null && IPAddress.TryParse(result.Ip, out var parsed) && parsed.AddressFamily != family)
            return null;

        return result;
    }

    /// <summary>Tries each endpoint in order, returning the first valid IP and any errors collected.</summary>
    private static async Task<(PublicIpResult? Result, List<string> Errors)> TryEndpointsAsync(
        HttpClient client, string[] endpoints, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await client.GetStringAsync(endpoint, cancellationToken).ConfigureAwait(false);
                var ip = response.Trim();
                if (IPAddress.TryParse(ip, out _))
                    return (new PublicIpResult(ip, endpoint), errors);
                errors.Add($"{endpoint}: unexpected response '{Truncate(ip)}'");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or SocketException)
            {
                errors.Add($"{endpoint}: {ex.Message}");
            }
        }
        return (null, errors);
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

    /// <summary>Disposes the internal client, unless one was supplied by the caller.</summary>
    public void Dispose()
    {
        if (_ownsClient)
            _http.Dispose();
    }
}

/// <summary>Public IP address plus the service that reported it.</summary>
public sealed record PublicIpResult(string Ip, string Source);

/// <summary>
/// The public IPv4 and IPv6 addresses. Either may be null when the machine has
/// no connectivity in that family.
/// </summary>
public sealed record PublicIpPair(PublicIpResult? V4, PublicIpResult? V6)
{
    /// <summary>True when neither family could be resolved.</summary>
    public bool IsEmpty => V4 is null && V6 is null;
}

/// <summary>A local IP address bound to a network interface.</summary>
public sealed record LocalAddress(string InterfaceName, string Address, bool IsIPv4);
