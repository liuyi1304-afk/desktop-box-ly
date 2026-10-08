using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using DesktopBoxLY.Models;
using DesktopBoxLY.Services;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DesktopBoxLY;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private static readonly Dictionary<Guid, BoxWindow> OpenBoxes = [];

    public static ObservableCollection<BoxConfig> Boxes { get; } = new(BoxStateStore.Load());
    public static MainWindow? MainWindow { get; private set; }

    public static void SaveBoxes() => BoxStateStore.Save(Boxes);

    public static void OpenBox(BoxConfig config)
    {
        if (OpenBoxes.TryGetValue(config.Id, out var existing))
        {
            existing.Activate();
            return;
        }

        var window = new BoxWindow(config);
        OpenBoxes[config.Id] = window;
        window.Closed += (_, _) => OpenBoxes.Remove(config.Id);
        window.Activate();
    }

    public static void CloseBox(Guid id)
    {
        if (OpenBoxes.TryGetValue(id, out var window)) window.Close();
    }
    
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        _window = MainWindow;
        _window.Activate();
        foreach (var box in Boxes.ToArray())
            if (Directory.Exists(box.FolderPath)) OpenBox(box);
    }
}
