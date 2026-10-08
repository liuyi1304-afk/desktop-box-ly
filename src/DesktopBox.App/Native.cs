using System.Runtime.InteropServices;
using System.Windows.Interop;
namespace DesktopBox;
internal static class Native {
 [DllImport("user32.dll")]public static extern bool RegisterHotKey(IntPtr h,int id,uint mods,uint key);
 [DllImport("user32.dll")]public static extern bool UnregisterHotKey(IntPtr h,int id);
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr h,int attr,ref int value,int size);
 [StructLayout(LayoutKind.Sequential)]struct Margins{public int Left,Right,Top,Bottom;}
 [DllImport("dwmapi.dll")]static extern int DwmExtendFrameIntoClientArea(IntPtr h,ref Margins margins);
 public static void Glass(System.Windows.Window window){var h=new WindowInteropHelper(window).Handle;var source=HwndSource.FromHwnd(h);if(source?.CompositionTarget!=null)source.CompositionTarget.BackgroundColor=System.Windows.Media.Colors.Transparent;var margins=new Margins{Left=-1,Right=-1,Top=-1,Bottom=-1};DwmExtendFrameIntoClientArea(h,ref margins);int backdrop=3;DwmSetWindowAttribute(h,38,ref backdrop,4);int corners=2;DwmSetWindowAttribute(h,33,ref corners,4);}
}
