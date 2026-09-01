using System.Net;
using System.Net.Sockets;
using Kaniff.Core.Tools;
using Xunit;

namespace Kaniff.Tests;

public class ToolTests
{
    [Fact]
    public void Base64_RoundTrips()
    {
        var tool = new Base64Tool();
        var encoded = tool.Encode("Kaniff rocks");
        Assert.Equal("Kaniff rocks", tool.Decode(encoded));
    }

    [Fact]
    public void Base64_UrlSafe_HasNoPaddingOrSlashes()
    {
        var encoded = new Base64Tool().Encode("???>>>", urlSafe: true);
        Assert.DoesNotContain('=', encoded);
        Assert.DoesNotContain('/', encoded);
        Assert.DoesNotContain('+', encoded);
    }

    [Fact]
    public void Case_ProducesExpectedStyles()
    {
        var r = new CaseTool().Convert("hello world example");
        Assert.Equal("helloWorldExample", r.Camel);
        Assert.Equal("HelloWorldExample", r.Pascal);
        Assert.Equal("hello_world_example", r.Snake);
        Assert.Equal("hello-world-example", r.Kebab);
        Assert.Equal("HELLO_WORLD_EXAMPLE", r.Constant);
    }

    [Fact]
    public void Case_SplitsCamelCaseInput()
    {
        var r = new CaseTool().Convert("helloWorldHttp");
        Assert.Equal("hello-world-http", r.Kebab);
    }

    [Fact]
    public void Color_ConvertsHexToRgbAndHsl()
    {
        var r = new ColorTool().Convert("#3498db");
        Assert.Equal("#3498DB", r.Hex);
        Assert.Equal("rgb(52, 152, 219)", r.Rgb);
        Assert.Equal("hsl(204, 70%, 53%)", r.Hsl);
    }

    [Fact]
    public void Color_ParsesRgbInput()
    {
        var r = new ColorTool().Convert("rgb(255, 0, 0)");
        Assert.Equal("#FF0000", r.Hex);
    }

    [Fact]
    public void Regex_ReturnsMatchesWithGroups()
    {
        var result = new RegexTool().Match(@"(\d)(\d)", "a12b34");
        Assert.Equal(2, result.Matches.Count);
        Assert.Equal("12", result.Matches[0].Value);
        Assert.Equal("1", result.Matches[0].Groups[1].Value);
    }

    [Fact]
    public void Json_FormatThenMinify_IsStable()
    {
        var tool = new JsonTool();
        var minified = tool.Minify(tool.Format("{ \"a\": 1, \"b\": [1, 2] }"));
        Assert.Equal("{\"a\":1,\"b\":[1,2]}", minified);
    }

    [Fact]
    public void Json_Validate_ReportsError()
    {
        Assert.NotNull(new JsonTool().Validate("{ not json"));
    }

    [Fact]
    public void Timestamp_FromUnixSeconds_MatchesIso()
    {
        var r = new TimestampTool().FromUnix(1700000000);
        Assert.Equal("2023-11-14T22:13:20Z", r.Iso8601);
        Assert.Equal(1700000000, r.UnixSeconds);
    }

    [Fact]
    public void Hash_Sha256_IsLowercaseHex()
    {
        var r = new HashTool().Compute("abc");
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", r.Sha256);
    }

    [Fact]
    public void Uuid_GeneratesRequestedCount()
    {
        var ids = new UuidTool().Generate(5);
        Assert.Equal(5, ids.Count);
        Assert.All(ids, id => Assert.True(Guid.TryParse(id, out _)));
    }

    [Fact]
    public void Url_RoundTrips()
    {
        var tool = new UrlEncodeTool();
        var encoded = tool.Encode("a b&c=1");
        Assert.Equal("a b&c=1", tool.Decode(encoded));
    }

    [Fact]
    public void StringCompare_FindsFirstDifference()
    {
        var r = new StringCompareTool().Compare("abc", "abd");
        Assert.False(r.AreEqual);
        Assert.Equal(2, r.FirstDifferenceIndex);
    }

    [Fact]
    public void Ip_LocalAddresses_IncludeIPv4()
    {
        using var tool = new IpTool();
        var addresses = tool.GetLocalAddresses();

        // Any machine running this test has at least one non-loopback IPv4
        // address, so an empty v4 set means the filter is broken.
        Assert.Contains(addresses, a => a.IsIPv4);
    }

    [Fact]
    public void Ip_LocalAddresses_ExcludeIPv6_WhenNotRequested()
    {
        using var tool = new IpTool();
        Assert.All(tool.GetLocalAddresses(includeIPv6: false), a => Assert.True(a.IsIPv4));
    }

    [Fact]
    public void Ip_PublicIpPair_ReportsEmptyOnlyWhenBothAreMissing()
    {
        Assert.True(new PublicIpPair(null, null).IsEmpty);
        Assert.False(new PublicIpPair(new PublicIpResult("203.0.113.1", "test"), null).IsEmpty);
        Assert.False(new PublicIpPair(null, new PublicIpResult("2001:db8::1", "test")).IsEmpty);
    }

    // The DNS and port tests below deliberately stay on loopback so the suite
    // never depends on an internet connection or on someone else's name server.

    [Fact]
    public async Task Dns_ResolvesLocalhost()
    {
        var result = await new DnsTool().LookupAsync("localhost");

        Assert.False(result.IsReverse);
        Assert.NotEmpty(result.Records);
        Assert.All(result.Records, r => Assert.True(IPAddress.TryParse(r.Address, out _)));
    }

    [Fact]
    public async Task Dns_TreatsAddressAsReverseLookup()
    {
        var result = await new DnsTool().LookupAsync("127.0.0.1");

        Assert.True(result.IsReverse);
        Assert.Empty(result.Records);
    }

    [Fact]
    public async Task Dns_StripsSchemeAndBrackets()
    {
        // A pasted URL and a bracketed literal are both things people type into
        // a lookup box, and neither should reach the resolver verbatim.
        Assert.Equal("localhost", (await new DnsTool().LookupAsync("http://localhost/path")).Query);
        Assert.True((await new DnsTool().LookupAsync("[::1]")).IsReverse);
    }

    [Fact]
    public async Task Dns_RejectsEmptyQuery()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new DnsTool().LookupAsync("   "));
    }

    [Fact]
    public async Task Dns_ReportsUnknownHost()
    {
        // .invalid is reserved by RFC 2606 and must never resolve.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new DnsTool().LookupAsync("kaniff-does-not-exist.invalid"));

        Assert.Contains("could not be found", ex.Message);
    }

    [Fact]
    public void DnsResult_SplitsRecordsByFamily()
    {
        var result = new DnsLookupResult(
            "example",
            "example",
            [new DnsRecord("203.0.113.1", true), new DnsRecord("2001:db8::1", false)],
            1,
            false);

        Assert.Equal("A", Assert.Single(result.IPv4).Kind);
        Assert.Equal("AAAA", Assert.Single(result.IPv6).Kind);
    }

    [Fact]
    public async Task Port_DetectsListeningPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var result = await new PortTool().CheckAsync("127.0.0.1", port);

            Assert.True(result.IsOpen);
            Assert.Equal(PortStatus.Open, result.Status);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task Port_DetectsClosedPort()
    {
        // Bind and release to obtain a port number that is very unlikely to be
        // taken again by the time the check runs.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var result = await new PortTool().CheckAsync("127.0.0.1", port, TimeSpan.FromSeconds(5));

        Assert.False(result.IsOpen);
        Assert.Equal(PortStatus.Closed, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    [InlineData(-1)]
    public async Task Port_RejectsOutOfRangePort(int port)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => new PortTool().CheckAsync("127.0.0.1", port));
    }

    [Fact]
    public async Task Port_RejectsEmptyHost()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new PortTool().CheckAsync(" ", 80));
    }

    [Fact]
    public void Port_NamesWellKnownServices()
    {
        Assert.Equal("HTTPS", PortTool.DescribePort(443));
        Assert.Equal("PostgreSQL", PortTool.DescribePort(5432));
        Assert.Null(PortTool.DescribePort(45678));
    }
}