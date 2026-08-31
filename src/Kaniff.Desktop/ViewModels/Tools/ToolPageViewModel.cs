namespace Kaniff.Desktop.ViewModels.Tools;

/// <summary>Base class for a tool page shown in the navigation content area.</summary>
public abstract class ToolPageViewModel : ViewModelBase
{
    protected ToolPageViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
