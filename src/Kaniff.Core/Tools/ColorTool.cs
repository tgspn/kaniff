using System.Globalization;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Converts colors between HEX, RGB and HSL representations.</summary>
public sealed class ColorTool : ITool
{
    public string Id => "color";
    public string Name => "Color Converter";
    public string Description => "Convert colors between HEX, RGB and HSL.";
    public ToolCategory Category => ToolCategory.Text;

    public ColorResult Convert(string input)
    {
        var (r, g, b) = Parse(input);
        var (h, s, l) = ToHsl(r, g, b);
        return new ColorResult(
            Hex: $"#{r:X2}{g:X2}{b:X2}",
            Rgb: $"rgb({r}, {g}, {b})",
            Hsl: $"hsl({Math.Round(h)}, {Math.Round(s * 100)}%, {Math.Round(l * 100)}%)");
    }

    private static (int R, int G, int B) Parse(string input)
    {
        var text = (input ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(text))
            throw new FormatException("Empty color.");

        if (text.StartsWith('#'))
            return ParseHex(text[1..]);

        if (text.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var inner = text[(text.IndexOf('(') + 1)..text.IndexOf(')')];
            var parts = inner.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                throw new FormatException("Expected rgb(r, g, b).");
            return (Clamp(parts[0]), Clamp(parts[1]), Clamp(parts[2]));
        }

        // Bare hex like "3498db".
        return ParseHex(text);
    }

    private static (int R, int G, int B) ParseHex(string hex)
    {
        if (hex.Length == 3)
            hex = string.Concat(hex.Select(c => new string(c, 2)));
        if (hex.Length is not (6 or 8))
            throw new FormatException("Hex color must have 3, 6 or 8 digits.");

        int Part(int start) => int.Parse(hex.Substring(start, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return (Part(0), Part(2), Part(4));
    }

    private static int Clamp(string value) => Math.Clamp(int.Parse(value, CultureInfo.InvariantCulture), 0, 255);

    private static (double H, double S, double L) ToHsl(int r, int g, int b)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd)), min = Math.Min(rd, Math.Min(gd, bd));
        double h = 0, s, l = (max + min) / 2;

        if (max == min)
        {
            s = 0;
        }
        else
        {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == rd) h = (gd - bd) / d + (gd < bd ? 6 : 0);
            else if (max == gd) h = (bd - rd) / d + 2;
            else h = (rd - gd) / d + 4;
            h *= 60;
        }
        return (h, s, l);
    }
}

/// <summary>A color expressed in several formats.</summary>
public sealed record ColorResult(string Hex, string Rgb, string Hsl);
