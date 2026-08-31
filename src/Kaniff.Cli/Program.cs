using System.Text;
using Kaniff.Core;
using Kaniff.Core.Tools;

return await KaniffCli.RunAsync(args);

internal static class KaniffCli
{
    public static async Task<int> RunAsync(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return 0;
        }

        var verb = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();

        try
        {
            return verb switch
            {
                "list" => List(),
                "ip" => await IpAsync(rest),
                "base64" or "b64" => Base64(rest),
                "url" => Url(rest),
                "jwt" => Jwt(rest),
                "hash" => Hash(rest),
                "uuid" or "guid" => Uuid(rest),
                "timestamp" or "ts" => Timestamp(rest),
                "case" => Case(rest),
                "color" => Color(rest),
                "regex" => Regex(rest),
                "qr" => Qr(rest),
                "strcmp" or "diff" => StringCompare(rest),
                "json" => Json(rest),
                _ => Unknown(verb)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static int List()
    {
        foreach (var tool in ToolCatalog.All)
            Console.WriteLine($"{tool.Id,-8} {tool.Name} — {tool.Description}");
        return 0;
    }

    private static async Task<int> IpAsync(string[] args)
    {
        var wantLocal = args.Contains("--local") || args.Contains("-l");
        var wantPublic = args.Contains("--public") || args.Contains("-p");
        if (!wantLocal && !wantPublic)
            wantLocal = wantPublic = true;

        var tool = new IpTool();

        if (wantPublic)
        {
            try
            {
                var result = await tool.GetPublicIpAsync();
                Console.WriteLine($"Public : {result.Ip}  (via {result.Source})");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Public : unavailable — {ex.Message}");
            }
        }

        if (wantLocal)
        {
            foreach (var addr in tool.GetLocalAddresses())
            {
                var kind = addr.IsIPv4 ? "IPv4" : "IPv6";
                Console.WriteLine($"Local  : {addr.Address}  ({kind}, {addr.InterfaceName})");
            }
        }

        return 0;
    }

    private static int Base64(string[] args)
    {
        if (args.Length == 0)
            return Fail("usage: kaniff base64 <encode|decode> [text] [--url-safe]");

        var op = args[0].ToLowerInvariant();
        var urlSafe = args.Contains("--url-safe") || args.Contains("-u");
        var input = ReadInput(args.Skip(1).Where(a => !a.StartsWith('-')));
        var tool = new Base64Tool();

        var output = op switch
        {
            "encode" or "enc" => tool.Encode(input, urlSafe),
            "decode" or "dec" => tool.Decode(input),
            _ => throw new ArgumentException($"unknown base64 operation '{op}' (use encode/decode)")
        };
        Console.WriteLine(output);
        return 0;
    }

    private static int Url(string[] args)
    {
        if (args.Length == 0)
            return Fail("usage: kaniff url <encode|decode> [text]");

        var op = args[0].ToLowerInvariant();
        var input = ReadInput(args.Skip(1));
        var tool = new UrlEncodeTool();

        var output = op switch
        {
            "encode" or "enc" => tool.Encode(input),
            "decode" or "dec" => tool.Decode(input),
            _ => throw new ArgumentException($"unknown url operation '{op}' (use encode/decode)")
        };
        Console.WriteLine(output);
        return 0;
    }

    private static int Hash(string[] args)
    {
        var input = ReadInput(args);
        var result = new HashTool().Compute(input);
        Console.WriteLine($"MD5     : {result.Md5}");
        Console.WriteLine($"SHA-1   : {result.Sha1}");
        Console.WriteLine($"SHA-256 : {result.Sha256}");
        Console.WriteLine($"SHA-512 : {result.Sha512}");
        return 0;
    }

    private static int Uuid(string[] args)
    {
        var uppercase = args.Contains("-u") || args.Contains("--upper");
        var countArg = args.FirstOrDefault(a => !a.StartsWith('-'));
        var count = int.TryParse(countArg, out var n) ? n : 1;
        foreach (var id in new UuidTool().Generate(count, uppercase))
            Console.WriteLine(id);
        return 0;
    }

    private static int Timestamp(string[] args)
    {
        var input = ReadInput(args);
        var tool = new TimestampTool();
        var result = string.IsNullOrEmpty(input)
            ? tool.Now()
            : long.TryParse(input, out var unix) ? tool.FromUnix(unix) : tool.FromDate(input);

        Console.WriteLine($"ISO 8601    : {result.Iso8601}");
        Console.WriteLine($"Local       : {result.Local}");
        Console.WriteLine($"Unix (s)    : {result.UnixSeconds}");
        Console.WriteLine($"Unix (ms)   : {result.UnixMilliseconds}");
        return 0;
    }

    private static int Case(string[] args)
    {
        var input = ReadInput(args);
        var r = new CaseTool().Convert(input);
        Console.WriteLine($"lower     : {r.Lower}");
        Console.WriteLine($"UPPER     : {r.Upper}");
        Console.WriteLine($"Title     : {r.Title}");
        Console.WriteLine($"camelCase : {r.Camel}");
        Console.WriteLine($"PascalCase: {r.Pascal}");
        Console.WriteLine($"snake_case: {r.Snake}");
        Console.WriteLine($"kebab-case: {r.Kebab}");
        Console.WriteLine($"CONSTANT  : {r.Constant}");
        return 0;
    }

    private static int Color(string[] args)
    {
        var input = ReadInput(args);
        var r = new ColorTool().Convert(input);
        Console.WriteLine($"HEX : {r.Hex}");
        Console.WriteLine($"RGB : {r.Rgb}");
        Console.WriteLine($"HSL : {r.Hsl}");
        return 0;
    }

    private static int Regex(string[] args)
    {
        var ignoreCase = args.Contains("-i") || args.Contains("--ignore-case");
        var multiline = args.Contains("-m") || args.Contains("--multiline");
        var operands = args.Where(a => !a.StartsWith('-')).ToArray();
        if (operands.Length < 2)
            return Fail("usage: kaniff regex <pattern> <input> [-i] [-m]");

        var result = new RegexTool().Match(operands[0], operands[1], ignoreCase, multiline);
        if (result.Matches.Count == 0)
        {
            Console.WriteLine("no matches");
            return 2;
        }
        Console.WriteLine($"{result.Matches.Count} match(es):");
        foreach (var m in result.Matches)
        {
            Console.WriteLine($"  [{m.Index}] '{m.Value}'");
            foreach (var g in m.Groups.Skip(1))
                Console.WriteLine($"      group {g.Name}: '{g.Value}' @ {g.Index}");
        }
        return 0;
    }

    private static int Qr(string[] args)
    {
        var pngPath = GetOption(args, "--png");
        var input = ReadInput(args.Where(a => !a.StartsWith('-') && a != pngPath));
        var tool = new QrTool();
        if (pngPath is not null)
        {
            File.WriteAllBytes(pngPath, tool.GeneratePng(input));
            Console.WriteLine($"saved {pngPath}");
        }
        else
        {
            Console.WriteLine(tool.GenerateAscii(input));
        }
        return 0;
    }

    private static string? GetOption(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static int Jwt(string[] args)
    {
        var tokenArgs = args.Where(a => !a.Equals("decode", StringComparison.OrdinalIgnoreCase));
        var token = ReadInput(tokenArgs);
        var result = new JwtTool().Decode(token);

        Console.WriteLine("== Header ==");
        Console.WriteLine(result.HeaderJson);
        Console.WriteLine();
        Console.WriteLine("== Payload ==");
        Console.WriteLine(result.PayloadJson);
        Console.WriteLine();
        if (result.IssuedAt is { } iat)
            Console.WriteLine($"Issued at : {iat:u}");
        if (result.NotBefore is { } nbf)
            Console.WriteLine($"Not before: {nbf:u}");
        if (result.ExpiresAt is { } exp)
            Console.WriteLine($"Expires at: {exp:u}  ({(result.IsExpired ? "EXPIRED" : "valid")})");
        return 0;
    }

    private static int StringCompare(string[] args)
    {
        var ignoreCase = args.Contains("-i") || args.Contains("--ignore-case");
        var ignoreWhitespace = args.Contains("-w") || args.Contains("--ignore-whitespace");
        var operands = args.Where(a => !a.StartsWith('-')).ToArray();
        if (operands.Length < 2)
            return Fail("usage: kaniff strcmp <a> <b> [-i] [-w]");

        var result = new StringCompareTool().Compare(operands[0], operands[1], ignoreCase, ignoreWhitespace);
        if (result.AreEqual)
        {
            Console.WriteLine("equal");
        }
        else
        {
            Console.WriteLine("different");
            Console.WriteLine($"first difference at index: {result.FirstDifferenceIndex}");
            Console.WriteLine($"lengths: {result.LeftLength} vs {result.RightLength}");
        }
        return result.AreEqual ? 0 : 2;
    }

    private static int Json(string[] args)
    {
        if (args.Length == 0)
            return Fail("usage: kaniff json <format|minify|validate> [json]");

        var op = args[0].ToLowerInvariant();
        var input = ReadInput(args.Skip(1));
        var tool = new JsonTool();

        switch (op)
        {
            case "format" or "pretty":
                Console.WriteLine(tool.Format(input));
                return 0;
            case "minify" or "min":
                Console.WriteLine(tool.Minify(input));
                return 0;
            case "validate" or "check":
                var error = tool.Validate(input);
                if (error is null)
                {
                    Console.WriteLine("valid");
                    return 0;
                }
                Console.Error.WriteLine($"invalid: {error}");
                return 2;
            default:
                return Fail($"unknown json operation '{op}' (use format/minify/validate)");
        }
    }

    /// <summary>Uses the joined arguments, or reads from stdin when no argument is given.</summary>
    private static string ReadInput(IEnumerable<string> args)
    {
        var joined = string.Join(' ', args).Trim();
        if (!string.IsNullOrEmpty(joined))
            return joined;
        return Console.IsInputRedirected ? Console.In.ReadToEnd().Trim() : string.Empty;
    }

    private static bool IsHelp(string arg) =>
        arg is "--help" or "-h" or "help";

    private static int Unknown(string verb)
    {
        Console.Error.WriteLine($"unknown command '{verb}'. Run 'kaniff help' for usage.");
        return 1;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
        Kaniff — your developer Swiss Army knife.

        Usage:
          kaniff <command> [options]

        Commands:
          ip [--local] [--public]         Show public and/or local IP addresses
          base64 encode <text> [-u]       Encode text to Base64 (-u = URL-safe)
          base64 decode <text>            Decode Base64 to text
          url encode|decode <text>        Percent-encode/decode for URLs
          jwt <token>                     Decode a JWT header and payload
          hash <text>                     MD5/SHA-1/SHA-256/SHA-512 of text
          uuid [count] [-u]               Generate random UUID(s)
          timestamp [unix|date]           Convert Unix time <-> date (now if empty)
          case <text>                     Convert between casing styles
          color <hex|rgb(...)>            Convert a color to HEX/RGB/HSL
          regex <pattern> <input> [-i]    Test a regex and list matches
          qr <text> [--png file.png]      Generate a QR code (ASCII or PNG)
          strcmp <a> <b> [-i] [-w]        Compare two strings (-i ignore case, -w ignore whitespace)
          json format <json>              Pretty-print JSON
          json minify <json>              Minify JSON
          json validate <json>            Validate JSON
          list                            List available tools

        Most text commands also read from stdin, e.g.:
          echo "aGVsbG8=" | kaniff base64 decode
        """);
    }
}
