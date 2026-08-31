using System.Globalization;
using Kaniff.Core.Abstractions;

namespace Kaniff.Core.Tools;

/// <summary>Converts between Unix timestamps and human-readable dates.</summary>
public sealed class TimestampTool : ITool
{
    public string Id => "timestamp";
    public string Name => "Timestamp Converter";
    public string Description => "Convert Unix time to a date and back (seconds or milliseconds).";
    public ToolCategory Category => ToolCategory.Text;

    /// <summary>Interprets a Unix timestamp (auto-detecting seconds vs milliseconds).</summary>
    public TimestampResult FromUnix(long value)
    {
        // Values past year ~2286 in seconds are almost certainly milliseconds.
        var isMillis = Math.Abs(value) > 100_000_000_000L;
        var instant = isMillis
            ? DateTimeOffset.FromUnixTimeMilliseconds(value)
            : DateTimeOffset.FromUnixTimeSeconds(value);
        return new TimestampResult(instant, isMillis ? "milliseconds" : "seconds");
    }

    /// <summary>Parses a date/time string (ISO 8601 preferred) into a Unix timestamp.</summary>
    public TimestampResult FromDate(string text)
    {
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var instant))
        {
            throw new FormatException($"Could not parse '{text}' as a date/time.");
        }
        return new TimestampResult(instant, "date");
    }

    public TimestampResult Now() => new(DateTimeOffset.UtcNow, "now");
}

/// <summary>A point in time with its various representations.</summary>
public sealed record TimestampResult(DateTimeOffset Instant, string DetectedUnit)
{
    public long UnixSeconds => Instant.ToUnixTimeSeconds();
    public long UnixMilliseconds => Instant.ToUnixTimeMilliseconds();
    public string Iso8601 => Instant.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    public string Local => Instant.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
}
