using QRCoder;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Generates QR codes as terminal-friendly ASCII or as PNG bytes.</summary>
public sealed class QrTool : ITool
{
    public string Id => "qr";
    public string Name => "QR Code";
    public string Description => "Generate a QR code from text (ASCII or PNG).";
    public ToolCategory Category => ToolCategory.Encoding;

    public string GenerateAscii(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(Require(text), QRCodeGenerator.ECCLevel.M);
        var ascii = new AsciiQRCode(data);
        return ascii.GetGraphic(1);
    }

    public byte[] GeneratePng(string text, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(Require(text), QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }

    private static string Require(string text) =>
        string.IsNullOrEmpty(text)
            ? throw new ArgumentException("Text is required to build a QR code.")
            : text;
}
