using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopBox;
internal static class ShellIcons {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct Info{public IntPtr Icon;public int Index;public uint Attributes;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string Name;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=80)]public string Type;}
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern IntPtr SHGetFileInfo(string path,uint attr,out Info info,uint size,uint flags);
 [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
 internal static ImageSource? Get(string path){SHGetFileInfo(path,0,out var info,(uint)Marshal.SizeOf<Info>(),0x100);if(info.Icon==IntPtr.Zero)return null;try{var image=Imaging.CreateBitmapSourceFromHIcon(info.Icon,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());image.Freeze();return image;}finally{DestroyIcon(info.Icon);}}
}
