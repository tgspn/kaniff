using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class StringCompareViewModel : ToolPageViewModel
{
    private readonly StringCompareTool _tool = new();

    public StringCompareViewModel() : base("String Comparer", "Compare two strings and find the first difference.") { }

    [ObservableProperty]
    public partial string Left { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Right { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IgnoreCase { get; set; }

    [ObservableProperty]
    public partial bool IgnoreWhitespace { get; set; }

    [ObservableProperty]
    public partial string Result { get; set; } = string.Empty;

    [RelayCommand]
    private void Compare()
    {
        var r = _tool.Compare(Left, Right, IgnoreCase, IgnoreWhitespace);
        if (r.AreEqual)
        {
            Result = "✔ Strings are equal.";
        }
        else
        {
            Result = $"✘ Different.\nFirst difference at index {r.FirstDifferenceIndex}.\nLengths: {r.LeftLength} vs {r.RightLength}.";
        }
    }
}
