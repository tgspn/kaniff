using System.Security.Cryptography;
using System.Text;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Computes common cryptographic hashes of a text input.</summary>
public sealed class HashTool : ITool
{
    public string Id => "hash";
    public string Name => "Hash";
    public string Description => "Compute MD5, SHA-1, SHA-256 and SHA-512 hashes of text.";
    public ToolCategory Category => ToolCategory.Security;

    public HashResult Compute(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(text ?? string.Empty);
        return new HashResult(
            Md5: ToHex(MD5.HashData(bytes)),
            Sha1: ToHex(SHA1.HashData(bytes)),
            Sha256: ToHex(SHA256.HashData(bytes)),
            Sha512: ToHex(SHA512.HashData(bytes)));
    }

    private static string ToHex(byte[] hash) => Convert.ToHexStringLower(hash);
}

/// <summary>Hex-encoded digests of an input.</summary>
public sealed record HashResult(string Md5, string Sha1, string Sha256, string Sha512);
