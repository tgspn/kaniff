using System.Reflection;
using Avalonia.Controls;

namespace Kaniff.Desktop.Views;

/// <summary>
/// Borderless window shown while the app starts up.
/// </summary>
public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        VersionText.Text = GetDisplayVersion();
    }

    /// <summary>
    /// Reads the informational version, which carries the full string produced by
    /// the build (including any suffix), falling back to the assembly version.
    /// </summary>
    private static string GetDisplayVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        // The SDK appends the source revision after a '+', which is noise here.
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return "v" + (plus >= 0 ? informational[..plus] : informational);
        }

        var version = assembly.GetName().Version;
        return version is null ? string.Empty : $"v{version.ToString(3)}";
    }
}
