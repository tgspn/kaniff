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
}
