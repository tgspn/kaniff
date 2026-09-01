using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class DnsViewModel : ToolPageViewModel
{
    private readonly DnsTool _tool = new();

    public DnsViewModel()
        : base("DNS Lookup", "Resolve a host name to IP addresses, or an IP address back to a name.")
    {
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LookupCommand))]
    public partial string Query { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LookupCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;

    /// <summary>True when the last lookup failed, so the view can colour the status line.</summary>
    [ObservableProperty]
    public partial bool HasError { get; set; }

    public ObservableCollection<DnsResultItem> Results { get; } = [];

    /// <summary>Every address found, one per line, for the "copy all" button.</summary>
    public string AllAddresses => string.Join(Environment.NewLine, Results.Select(r => r.Value));

    /// <summary>
    /// A single result already has its own copy button on the row, so the "copy all"
    /// button would duplicate it. Only offer it when it actually does something more.
    /// </summary>
    public bool ShowCopyAll => Results.Count > 1;

    private bool CanLookup => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanLookup))]
    private async Task LookupAsync()
    {
        IsBusy = true;
        HasError = false;
        Status = "Resolving…";
        Results.Clear();

        try
        {
            var result = await _tool.LookupAsync(Query);

            if (result.IsReverse)
            {
                if (result.CanonicalName is null)
                {
                    HasError = true;
                    Status = $"No PTR record for {result.Query}.";
                    return;
                }

                Results.Add(new DnsResultItem("PTR", result.CanonicalName));
            }
            else
            {
                foreach (var record in result.Records)
                    Results.Add(new DnsResultItem(record.Kind, record.Address));

                if (Results.Count == 0)
                {
                    HasError = true;
                    Status = $"No addresses found for {result.Query}.";
                    return;
                }
            }

            var label = Results.Count == 1 ? "result" : "results";
            Status = $"{Results.Count} {label} in {result.ElapsedMilliseconds} ms";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            HasError = true;
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(AllAddresses));
            OnPropertyChanged(nameof(ShowCopyAll));
        }
    }
}

/// <summary>One row of a lookup: the record type and the value it resolved to.</summary>
public sealed record DnsResultItem(string Kind, string Value);
