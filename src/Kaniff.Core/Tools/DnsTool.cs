using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Resolves host names to addresses, and addresses back to host names.</summary>
public sealed class DnsTool : ITool
{
    public string Id => "dns";
    public string Name => "DNS Lookup";
    public string Description => "Resolve a host name to IP addresses, or an IP address back to a name.";
    public ToolCategory Category => ToolCategory.Network;

    /// <summary>
    /// Looks up <paramref name="query"/>. An IP address triggers a reverse lookup,
    /// anything else a forward lookup, which mirrors what nslookup does and saves
    /// the caller from having to decide.
    /// </summary>
    /// <exception cref="ArgumentException">The query is empty.</exception>
    public async Task<DnsLookupResult> LookupAsync(string query, CancellationToken cancellationToken = default)
    {
        var host = (query ?? string.Empty).Trim();
        if (host.Length == 0)
            throw new ArgumentException("Host name or IP address is required.", nameof(query));

        // A URL is a common paste; take its host so the lookup does not fail on
        // something the user reasonably considers a host name.
        if (host.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            host = uri.Host;
        }

        // Bracketed IPv6 literals ("[::1]") arrive from URLs and connection strings.
        if (host.StartsWith('[') && host.EndsWith(']'))
            host = host[1..^1];

        var isAddress = IPAddress.TryParse(host, out var parsed);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (isAddress)
            {
                // The string overload reverse-resolves an IP literal just like the
                // IPAddress one, and is the only form that takes a cancellation token.
                var entry = await Dns.GetHostEntryAsync(host, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();

                // A reverse lookup with no PTR record returns the address back as
                // the name; reporting that as a hostname would be misleading.
                // Compare against the parsed form so that differently-cased or
                // abbreviated IPv6 input still matches.
                var name = string.Equals(entry.HostName, parsed!.ToString(), StringComparison.OrdinalIgnoreCase)
                    ? null
                    : entry.HostName;

                return new DnsLookupResult(host, name, [], stopwatch.ElapsedMilliseconds, IsReverse: true);
            }

            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            var records = addresses
                // IPv4 first: it is what most people are looking for, and the OS
                // does not guarantee an order.
                .OrderBy(a => a.AddressFamily == AddressFamily.InterNetwork ? 0 : 1)
                .ThenBy(a => a.ToString(), StringComparer.Ordinal)
                .Select(a => new DnsRecord(a.ToString(), a.AddressFamily == AddressFamily.InterNetwork))
                .ToArray();

            return new DnsLookupResult(host, host, records, stopwatch.ElapsedMilliseconds, IsReverse: false);
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            throw new InvalidOperationException(Describe(ex, host), ex);
        }
    }

    /// <summary>
    /// Turns socket error codes into something a user can act on. The default
    /// messages are phrased for network programmers, not for someone who mistyped
    /// a host name.
    /// </summary>
    private static string Describe(SocketException ex, string host) => ex.SocketErrorCode switch
    {
        SocketError.HostNotFound => $"'{host}' could not be found. Check the spelling.",
        SocketError.NoData => $"'{host}' exists but has no address record of the requested type.",
        SocketError.TryAgain => $"The name server is busy or unreachable. Try '{host}' again shortly.",
        _ => $"Lookup of '{host}' failed: {ex.Message}",
    };
}

/// <summary>A single address returned by a forward lookup.</summary>
/// <param name="Address">The address in its canonical text form.</param>
/// <param name="IsIPv4">True for A records, false for AAAA records.</param>
public sealed record DnsRecord(string Address, bool IsIPv4)
{
    public string Kind => IsIPv4 ? "A" : "AAAA";
}

/// <summary>Outcome of a DNS lookup.</summary>
/// <param name="Query">What was looked up, after normalisation.</param>
/// <param name="CanonicalName">Resolved name, or null when a reverse lookup found no PTR record.</param>
/// <param name="Records">Addresses found by a forward lookup; empty for a reverse lookup.</param>
/// <param name="ElapsedMilliseconds">How long the lookup took, including cached responses.</param>
/// <param name="IsReverse">True when an address was resolved back to a name.</param>
public sealed record DnsLookupResult(
    string Query,
    string? CanonicalName,
    IReadOnlyList<DnsRecord> Records,
    long ElapsedMilliseconds,
    bool IsReverse)
{
    public IEnumerable<DnsRecord> IPv4 => Records.Where(r => r.IsIPv4);

    public IEnumerable<DnsRecord> IPv6 => Records.Where(r => !r.IsIPv4);
}
