using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopBox;
internal static class AppIcons {
 internal static ImageSource WindowIcon {get;}=BitmapFrame.Create(new Uri("pack://application:,,,/Assets/App.ico"));
 internal static System.Drawing.Icon TrayIcon(){using var resource=Application.GetResourceStream(new Uri("/Assets/App.ico",UriKind.Relative)).Stream;using var icon=new System.Drawing.Icon(resource);return (System.Drawing.Icon)icon.Clone();}
}
