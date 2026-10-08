using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopBox;
internal static class ShellIcons {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct Info{public IntPtr Icon;public int Index;public uint Attributes;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string Name;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=80)]public string Type;}
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern IntPtr SHGetFileInfo(string path,uint attr,out Info info,uint size,uint flags);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode,EntryPoint="SHGetFileInfoW")]static extern IntPtr SHGetFileInfoPidl(IntPtr path,uint attr,out Info info,uint size,uint flags);
 [DllImport("shell32.dll")]static extern int SHGetImageList(int size,in Guid iid,out IntPtr list);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int GetIcon(IntPtr self,int index,int flags,out IntPtr icon);
 [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
 internal static ImageSource? Get(string path,int pixels=48){Info info;if(ShellItems.IsVirtual(path)){if(ShellItems.SHParseDisplayName(path,IntPtr.Zero,out var pidl,0,out _) <0)return null;try{SHGetFileInfoPidl(pidl,0,out info,(uint)Marshal.SizeOf<Info>(),0x4148);}finally{Marshal.FreeCoTaskMem(pidl);}}else SHGetFileInfo(path,0,out info,(uint)Marshal.SizeOf<Info>(),0x4140);if(info.Icon==IntPtr.Zero)return null;var iid=new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");if(SHGetImageList(pixels<=48?2:4,in iid,out var list)>=0){try{var method=Marshal.GetDelegateForFunctionPointer<GetIcon>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(list),10*IntPtr.Size));if(method(list,info.Index&0xFFFFFF,1|((info.Index>>24)<<8),out var large)>=0&&large!=IntPtr.Zero){DestroyIcon(info.Icon);info.Icon=large;}}finally{Marshal.Release(list);}}try{var image=Imaging.CreateBitmapSourceFromHIcon(info.Icon,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());image.Freeze();return image;}finally{DestroyIcon(info.Icon);}}
}
