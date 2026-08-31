using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Kaniff.Desktop.Controls;

/// <summary>
/// A button that copies <see cref="Text"/> to the clipboard and briefly confirms it.
/// </summary>
/// <remarks>
/// The clipboard is reached through <see cref="TopLevel"/>, which a view model has no
/// access to. Keeping the behaviour here lets any tool page offer a copy button with
/// one line of XAML, instead of every view model growing clipboard plumbing.
/// </remarks>
public class CopyButton : Button
{
    private const string IdleLabel = "Copy";
    private const string SuccessLabel = "Copied";
    private const string FailureLabel = "Failed";

    private static readonly TimeSpan FeedbackDuration = TimeSpan.FromSeconds(1.2);

    private CancellationTokenSource? _resetCts;

    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CopyButton, string?>(nameof(Text));

    /// <summary>The value placed on the clipboard when the button is pressed.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // Use the stock Button theme rather than looking for a CopyButton one that
    // does not exist, which would leave the control unstyled.
    protected override Type StyleKeyOverride => typeof(Button);

    public CopyButton()
    {
        Content = IdleLabel;

        // The label swaps between "Copy" and "Copied", so reserve the wider of
        // the two up front and the surrounding layout will not jump on click.
        MinWidth = 78;
    }

    protected override void OnClick()
    {
        base.OnClick();
        _ = CopyAsync();
    }

    private async Task CopyAsync()
    {
        var text = Text;
        if (string.IsNullOrEmpty(text))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        string label;
        try
        {
            // Avalonia 12 replaced SetTextAsync with a DataTransfer. Per its own
            // docs the object handed to SetDataAsync must NOT be disposed here:
            // the system owns it and releases it once the clipboard moves on.
            var data = new DataTransfer();
            data.Add(DataTransferItem.CreateText(text));

            await clipboard.SetDataAsync(data);
            label = SuccessLabel;
        }
        catch (Exception)
        {
            // Another process can hold the clipboard open, and on X11 it may be
            // unavailable entirely. Report it in the label; a copy button is not
            // worth tearing the app down for.
            label = FailureLabel;
        }

        Content = label;
        ResetLabelAfterDelay();
    }

    private void ResetLabelAfterDelay()
    {
        // Restart the countdown so rapid clicks do not clear the confirmation early.
        _resetCts?.Cancel();
        _resetCts?.Dispose();

        var cts = new CancellationTokenSource();
        _resetCts = cts;

        _ = ResetLabelAsync(cts.Token);
    }

    private async Task ResetLabelAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(FeedbackDuration, cancellationToken);
            Content = IdleLabel;
        }
        catch (OperationCanceledException)
        {
            // Superseded by a later click, which owns the label now.
        }
    }
}
