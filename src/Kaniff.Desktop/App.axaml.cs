using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Kaniff.Desktop.ViewModels;
using Kaniff.Desktop.Views;

namespace Kaniff.Desktop;

public partial class App : Application
{
    /// <summary>
    /// Minimum time the splash stays up, so it does not flash past as a glitch
    /// on a warm start.
    /// <para>
    /// Kept deliberately short. With ReadyToRun the main window is ready about
    /// 400 ms after the splash appears, so anything larger stops covering real
    /// work and simply delays the app: a 900 ms floor pushed the main window
    /// from 1282 ms out to 1704 ms.
    /// </para>
    /// </summary>
    private static readonly TimeSpan MinimumSplashDuration = TimeSpan.FromMilliseconds(250);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Without this the app would exit the moment the splash closes, since
            // the main window does not exist yet and the default shutdown mode
            // triggers on the last window closing.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashWindow();
            splash.Show();

            var shownAt = DateTime.UtcNow;

            // Queued at Background priority so the splash gets a render pass first.
            // Building the main window before this point would block the UI thread
            // and leave the splash invisible for the period it exists to cover.
            Dispatcher.UIThread.Post(
                () => _ = ShowMainWindowAsync(desktop, splash, shownAt),
                DispatcherPriority.Background);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task ShowMainWindowAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        SplashWindow splash,
        DateTime shownAt)
    {
        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(),
        };

        var remaining = MinimumSplashDuration - (DateTime.UtcNow - shownAt);
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining);

        desktop.MainWindow = mainWindow;
        mainWindow.Show();

        // Restore normal behaviour only once a real window exists, so closing it
        // ends the app as a user expects.
        desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

        splash.Close();
    }
}