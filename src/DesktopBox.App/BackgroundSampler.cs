using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
namespace DesktopBox;
internal static class BackgroundSampler {
 [StructLayout(LayoutKind.Sequential)] struct Rect{public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out Rect rect);
 [DllImport("user32.dll",SetLastError=true)]static extern bool SetWindowDisplayAffinity(IntPtr h,uint affinity);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr h);
 internal static string Info(Window w){var h=new WindowInteropHelper(w).Handle;GetWindowRect(h,out var r);return $"{r.Left},{r.Top},{r.Right},{r.Bottom} visible={IsWindowVisible(h)} dpi={System.Windows.Media.VisualTreeHelper.GetDpi(w).DpiScaleX}";}
 [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr handle);
 internal static bool SetExcluded(Window window,bool excluded){var h=new WindowInteropHelper(window).Handle;return h==IntPtr.Zero||SetWindowDisplayAffinity(h,excluded?0x11u:0u);}
 internal static BitmapSource Read(Window window){var h=new WindowInteropHelper(window).Handle;if(!GetWindowRect(h,out var rect))throw new InvalidOperationException("无法读取背景区域");int w=rect.Right-rect.Left,hg=rect.Bottom-rect.Top;if(w<=0||hg<=0||w>8000||hg>8000)throw new InvalidOperationException("背景区域大小不可用");using var bitmap=new System.Drawing.Bitmap(w,hg,System.Drawing.Imaging.PixelFormat.Format32bppRgb);using(var graphics=System.Drawing.Graphics.FromImage(bitmap))graphics.CopyFromScreen(rect.Left,rect.Top,0,0,bitmap.Size,System.Drawing.CopyPixelOperation.SourceCopy);var native=bitmap.GetHbitmap();try{var source=Imaging.CreateBitmapSourceFromHBitmap(native,IntPtr.Zero,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());source.Freeze();return source;}finally{DeleteObject(native);}}
}


