using System.Diagnostics;
using System.Net.Sockets;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>
/// Checks whether a TCP port accepts connections, covering what people actually
/// use "telnet host port" for. It deliberately does not implement the telnet
/// protocol: the goal is answering "is this port open?", not driving a terminal
/// session.
/// </summary>
public sealed class PortTool : ITool
{
    /// <summary>
    /// Default timeout. A firewall that drops packets silently leaves the socket
    /// waiting for the OS retransmit limit, which is around 21 seconds on Windows
    /// and far too long to sit in front of.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Well-known ports, so results can be labelled with the service people expect.</summary>
    private static readonly Dictionary<int, string> WellKnownPorts = new()
    {
        [20] = "FTP data",
        [21] = "FTP",
        [22] = "SSH",
        [23] = "Telnet",
        [25] = "SMTP",
        [53] = "DNS",
        [80] = "HTTP",
        [110] = "POP3",
        [143] = "IMAP",
        [389] = "LDAP",
        [443] = "HTTPS",
        [445] = "SMB",
        [587] = "SMTP submission",
        [993] = "IMAPS",
        [995] = "POP3S",
        [1433] = "SQL Server",
        [1521] = "Oracle",
        [3306] = "MySQL",
        [3389] = "RDP",
        [5432] = "PostgreSQL",
        [5672] = "AMQP",
        [6379] = "Redis",
        [8080] = "HTTP alternate",
        [8443] = "HTTPS alternate",
        [9200] = "Elasticsearch",
        [27017] = "MongoDB",
    };

    public string Id => "port";
    public string Name => "Port Check";
    public string Description => "Test whether a TCP port is open on a host (what telnet host port is used for).";
    public ToolCategory Category => ToolCategory.Network;

    /// <summary>Returns the conventional service name for a port, or null if unknown.</summary>
    public static string? DescribePort(int port) =>
        WellKnownPorts.TryGetValue(port, out var name) ? name : null;

    /// <summary>
    /// Attempts a TCP connection to <paramref name="host"/> on <paramref name="port"/>.
    /// </summary>
    /// <returns>
    /// The outcome, including how long it took. A failure to connect is a normal
    /// result rather than an exception: "closed" is an answer, not an error.
    /// </returns>
    /// <exception cref="ArgumentException">The host is empty or the port is out of range.</exception>
    public async Task<PortCheckResult> CheckAsync(
        string host,
        int port,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var target = (host ?? string.Empty).Trim();
        if (target.Length == 0)
            throw new ArgumentException("Host is required.", nameof(host));

        if (port is < 1 or > 65535)
            throw new ArgumentException($"Port must be between 1 and 65535, got {port}.", nameof(port));

        if (target.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(target, UriKind.Absolute, out var uri))
        {
            target = uri.Host;
        }

        if (target.StartsWith('[') && target.EndsWith(']'))
            target = target[1..^1];

        var limit = timeout ?? DefaultTimeout;
        var stopwatch = Stopwatch.StartNew();

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(limit);

        using var client = new TcpClient();

        try
        {
            await client.ConnectAsync(target, port, timeoutSource.Token).ConfigureAwait(false);
            stopwatch.Stop();

            return new PortCheckResult(
                target, port, PortStatus.Open, stopwatch.ElapsedMilliseconds,
                $"Connected to {target}:{port}.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The linked source fired, so this is our timeout rather than the
            // caller cancelling. No response at all usually means a firewall is
            // dropping packets instead of refusing the connection.
            stopwatch.Stop();
            return new PortCheckResult(
                target, port, PortStatus.TimedOut, stopwatch.ElapsedMilliseconds,
                $"No response within {limit.TotalSeconds:0.#}s. The port is filtered, or the host is unreachable.");
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();

            // A refused connection proves the host is up and reachable, which is
            // useful to say out loud: it is a different situation from a timeout.
            var status = ex.SocketErrorCode == SocketError.ConnectionRefused
                ? PortStatus.Closed
                : PortStatus.Unreachable;

            return new PortCheckResult(target, port, status, stopwatch.ElapsedMilliseconds, Describe(ex, target, port));
        }
    }

    private static string Describe(SocketException ex, string host, int port) => ex.SocketErrorCode switch
    {
        SocketError.ConnectionRefused =>
            $"{host} is reachable but nothing is listening on port {port}.",
        SocketError.HostNotFound =>
            $"'{host}' could not be found. Check the spelling.",
        SocketError.NetworkUnreachable or SocketError.HostUnreachable =>
            $"{host} is unreachable. Check your network connection.",
        _ => $"Could not connect to {host}:{port} — {ex.Message}",
    };
}

/// <summary>Outcome of a TCP port check.</summary>
public enum PortStatus
{
    /// <summary>The connection succeeded; something is listening.</summary>
    Open,

    /// <summary>The host actively refused the connection, so it is up but the port is closed.</summary>
    Closed,

    /// <summary>No response before the timeout, which typically means a firewall is dropping packets.</summary>
    TimedOut,

    /// <summary>The host could not be reached or resolved at all.</summary>
    Unreachable,
}

/// <summary>Result of checking a single TCP port.</summary>
/// <param name="Host">The host that was checked, after normalisation.</param>
/// <param name="Port">The port that was checked.</param>
/// <param name="Status">What happened.</param>
/// <param name="ElapsedMilliseconds">How long the attempt took.</param>
/// <param name="Message">A human-readable explanation of the outcome.</param>
public sealed record PortCheckResult(
    string Host,
    int Port,
    PortStatus Status,
    long ElapsedMilliseconds,
    string Message)
{
    public bool IsOpen => Status == PortStatus.Open;

    /// <summary>Conventional service name for the port, or null when it is not well known.</summary>
    public string? ServiceName => PortTool.DescribePort(Port);
}
