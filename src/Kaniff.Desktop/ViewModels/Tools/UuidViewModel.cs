using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class UuidViewModel : ToolPageViewModel
{
    private readonly UuidTool _tool = new();

    public UuidViewModel() : base("UUID", "Generate random version-4 UUIDs.")
    {
        Generate();
    }

    [ObservableProperty]
    public partial int Count { get; set; } = 1;

    [ObservableProperty]
    public partial bool Uppercase { get; set; }

    [ObservableProperty]
    public partial string Output { get; set; } = string.Empty;

    [RelayCommand]
    private void Generate()
    {
        var count = Count < 1 ? 1 : Count;
        Output = string.Join(Environment.NewLine, _tool.Generate(count, Uppercase));
    }
}
