using DesktopBoxLY.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DesktopBoxLY;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += (_, _) => RefreshList();
    }

    private void RefreshList()
    {
        BoxesList.ItemsSource = null;
        BoxesList.ItemsSource = App.Boxes;
        EmptyHint.Visibility = App.Boxes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void AddBox_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow!));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return;

        var nameDialog = new ContentDialog
        {
            Title = "给盒子命名",
            PrimaryButtonText = "创建",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };
        var nameInput = new TextBox { Text = folder.Name, PlaceholderText = "例如：工作资料" };
        nameDialog.Content = nameInput;
        if (await nameDialog.ShowAsync() != ContentDialogResult.Primary) return;

        var config = new BoxConfig
        {
            Name = string.IsNullOrWhiteSpace(nameInput.Text) ? folder.Name : nameInput.Text.Trim(),
            FolderPath = folder.Path
        };
        App.Boxes.Add(config);
        App.SaveBoxes();
        RefreshList();
        App.OpenBox(config);
    }

    private void OpenBox_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is Guid id && App.Boxes.FirstOrDefault(b => b.Id == id) is { } box)
            App.OpenBox(box);
    }

    private async void RemoveBox_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not Guid id || App.Boxes.FirstOrDefault(b => b.Id == id) is not { } box)
            return;

        var dialog = new ContentDialog
        {
            Title = $"移除“{box.Name}”？",
            Content = "这只会删除盒子配置，不会删除映射文件夹或其中的文件。",
            PrimaryButtonText = "移除盒子",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        App.CloseBox(box.Id);
        App.Boxes.Remove(box);
        App.SaveBoxes();
        RefreshList();
    }
}
