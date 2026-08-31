using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class HashViewModel : ToolPageViewModel
{
    private readonly HashTool _tool = new();

    public HashViewModel() : base("Hash", "Compute MD5, SHA-1, SHA-256 and SHA-512 hashes.") { }

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Md5 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sha1 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sha256 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sha512 { get; set; } = string.Empty;

    [RelayCommand]
    private void Compute()
    {
        var result = _tool.Compute(Input);
        Md5 = result.Md5;
        Sha1 = result.Sha1;
        Sha256 = result.Sha256;
        Sha512 = result.Sha512;
    }
}
