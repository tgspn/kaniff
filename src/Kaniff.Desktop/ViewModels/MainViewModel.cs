using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Kaniff.Desktop.ViewModels.Tools;

namespace Kaniff.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Tools =
        [
            new IpViewModel(),
            new Base64ViewModel(),
            new UrlViewModel(),
            new JwtViewModel(),
            new HashViewModel(),
            new UuidViewModel(),
            new TimestampViewModel(),
            new CaseViewModel(),
            new ColorViewModel(),
            new RegexViewModel(),
            new QrViewModel(),
            new StringCompareViewModel(),
            new JsonViewModel()
        ];
        SelectedTool = Tools[0];
    }

    public ObservableCollection<ToolPageViewModel> Tools { get; }

    [ObservableProperty]
    public partial ToolPageViewModel? SelectedTool { get; set; }
}
