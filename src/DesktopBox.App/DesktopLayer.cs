using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
namespace DesktopBox;
internal sealed class DesktopLayer:IDisposable {
 [StructLayout(LayoutKind.Sequential)]struct Rect{public int Left,Top,Right,Bottom;}
 delegate bool VisitWindow(IntPtr window,IntPtr param);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string kind,string? title);
 [DllImport("user32.dll")]static extern bool EnumWindows(VisitWindow visit,IntPtr param);
 [DllImport("user32.dll")]static extern bool ShowWindow(IntPtr window,int command);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr window);
 [DllImport("user32.dll")]static extern bool IsWindow(IntPtr window);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetClassName(IntPtr window,StringBuilder text,int count);
 [DllImport("user32.dll")]static extern IntPtr GetParent(IntPtr window);
 [DllImport("user32.dll")]static extern bool GetClientRect(IntPtr window,out Rect rect);
 [DllImport("user32.dll",SetLastError=true)]static extern IntPtr SetParent(IntPtr child,IntPtr parent);
 [DllImport("user32.dll")]static extern int GetWindowLong(IntPtr window,int index);
 [DllImport("user32.dll")]static extern int SetWindowLong(IntPtr window,int index,int value);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int width,int height,uint flags);
 static string Marker=>Path.Combine(Path.GetDirectoryName(Store.PathName)!,"desktop-layer-recovery.json");
 internal sealed record Recovery(long Handle);
 static string Class(IntPtr window){var text=new StringBuilder(128);GetClassName(window,text,text.Capacity);return text.ToString();}
 static IntPtr FindList(){IntPtr list=IntPtr.Zero;EnumWindows((window,_)=>{var view=FindWindowEx(window,IntPtr.Zero,"SHELLDLL_DefView",null);if(view==IntPtr.Zero)return true;list=FindWindowEx(view,IntPtr.Zero,"SysListView32",null);return list==IntPtr.Zero;},IntPtr.Zero);return list;}
 internal static bool NativeVisible {get{var list=FindList();return list!=IntPtr.Zero&&IsWindowVisible(list);}}
 internal static void RestoreNative(){if(!File.Exists(Marker))return;var record=JsonSerializer.Deserialize<Recovery>(File.ReadAllText(Marker))??throw new IOException("桌面恢复记录无效");var list=new IntPtr(record.Handle);if(!IsWindow(list)||Class(list)!="SysListView32"||Class(GetParent(list))!="SHELLDLL_DefView")list=FindList();if(list==IntPtr.Zero)throw new IOException("Explorer 桌面暂不可用，恢复记录已保留");ShowWindow(list,8);if(!IsWindowVisible(list))throw new IOException("系统桌面尚未恢复，恢复记录已保留");File.Delete(Marker);}
 readonly Func<Settings> settings;Window? window;readonly Canvas icons=new();IntPtr listHandle;string layoutKey="";bool disposed;
 internal DesktopLayer(Func<Settings> settings){this.settings=settings;}
 internal int DisplayedCount=>icons.Children.Count;
 internal bool Contains(string id)=>icons.Children.OfType<FrameworkElement>().Any(x=>string.Equals(x.Tag as string,id,StringComparison.OrdinalIgnoreCase));
 internal double RenderedIconPixels=>icons.Children.OfType<Border>().Select(x=>((StackPanel)x.Child).Children.OfType<Image>().First().Width*(window==null?1:VisualTreeHelper.GetDpi(window).DpiScaleX)).FirstOrDefault();
 internal bool Attached=>window!=null&&GetParent(new WindowInteropHelper(window).Handle)==GetParent(listHandle);
 internal void Refresh(){if(disposed)return;var collected=settings().Boxes.Where(x=>x.Folder==null).SelectMany(x=>x.Entries).ToHashSet(StringComparer.OrdinalIgnoreCase);if(collected.Count==0){Stop();return;}var items=DesktopIcons.ReadAll();if(!items.Any(x=>collected.Contains(x.Id))){Stop();return;}var native=FindList();if(native==IntPtr.Zero)throw new IOException("Explorer 桌面暂不可用");if(window!=null&&native!=listHandle){window.Close();window=null;layoutKey="";listHandle=IntPtr.Zero;}
  if(window==null){if(!IsWindowVisible(native)&&!File.Exists(Marker))return;listHandle=native;Directory.CreateDirectory(Path.GetDirectoryName(Marker)!);var temp=Marker+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(new Recovery(listHandle.ToInt64())));File.Move(temp,Marker,true);window=new Window{Title="桌面盒子 · 桌面图标层",WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,AllowsTransparency=true,Background=Brushes.Transparent,ShowInTaskbar=false,ShowActivated=false,Content=icons,Left=0,Top=0};window.SourceInitialized+=(_,_)=>{var handle=new WindowInteropHelper(window).Handle;var style=GetWindowLong(handle,-16);SetWindowLong(handle,-16,(style&~unchecked((int)0x80000000))|0x40000000);SetParent(handle,GetParent(listHandle));if(GetParent(handle)!=GetParent(listHandle))throw new IOException("系统拒绝桌面图标层嵌入");HwndSource.FromHwnd(handle).AddHook((IntPtr h,int msg,IntPtr w,IntPtr l,ref bool handled)=>{if(msg==0x21){handled=true;return new IntPtr(3);}return IntPtr.Zero;});};try{window.Show();}catch{Stop();throw;}}
  GetClientRect(listHandle,out var rect);var dpi=VisualTreeHelper.GetDpi(window);window.Width=Math.Max(1,(rect.Right-rect.Left)/dpi.DpiScaleX);window.Height=Math.Max(1,(rect.Bottom-rect.Top)/dpi.DpiScaleY);SetWindowPos(new WindowInteropHelper(window).Handle,IntPtr.Zero,0,0,rect.Right-rect.Left,rect.Bottom-rect.Top,0x54);
  var size=DesktopIcons.IconSize();var visible=items.Where(x=>!collected.Contains(x.Id)).ToArray();var key=$"{size}:{dpi.DpiScaleX}|"+string.Join("|",visible.Select(x=>$"{x.Id}:{x.Position.X},{x.Position.Y}"));if(key!=layoutKey){icons.Children.Clear();foreach(var item in visible)Add(item,dpi,size);layoutKey=key;}ShowWindow(listHandle,0);if(IsWindowVisible(listHandle)){Stop();throw new IOException("系统不允许隐藏原桌面图标层");}
 }
 void Add(DesktopIcons.DesktopItem item,DpiScale dpi,int size){var panel=new StackPanel{Width=Math.Max(86,(size+20)/dpi.DpiScaleX),Margin=new Thickness(0,4,0,0)};panel.Children.Add(new Image{Source=ShellIcons.Get(item.Id,size),Width=size/dpi.DpiScaleX,Height=size/dpi.DpiScaleY,Margin=new Thickness(0,0,0,5)});panel.Children.Add(new TextBlock{Text=item.Name,Foreground=Brushes.White,FontSize=12,TextAlignment=TextAlignment.Center,TextWrapping=TextWrapping.Wrap,MaxHeight=34,TextTrimming=TextTrimming.CharacterEllipsis,Effect=new System.Windows.Media.Effects.DropShadowEffect{BlurRadius=3,ShadowDepth=1,Opacity=0.9}});var tile=new Border{Child=panel,Background=Brushes.Transparent,CornerRadius=new CornerRadius(4),Tag=item.Id,ToolTip=item.Name};Canvas.SetLeft(tile,item.Position.X/dpi.DpiScaleX);Canvas.SetTop(tile,item.Position.Y/dpi.DpiScaleY);Point origin=default;bool armed=false;tile.MouseLeftButtonDown+=(_,e)=>{armed=e.ClickCount==1;origin=e.GetPosition(tile);if(armed)tile.CaptureMouse();if(e.ClickCount==2){Open(item.Id);e.Handled=true;}};tile.MouseLeftButtonUp+=(_,_)=>{armed=false;tile.ReleaseMouseCapture();};tile.LostMouseCapture+=(_,_)=>armed=false;tile.MouseMove+=(_,e)=>{if(!armed||e.LeftButton!=MouseButtonState.Pressed)return;var now=e.GetPosition(tile);if(Math.Abs(now.X-origin.X)<SystemParameters.MinimumHorizontalDragDistance&&Math.Abs(now.Y-origin.Y)<SystemParameters.MinimumVerticalDragDistance)return;armed=false;tile.ReleaseMouseCapture();if(!TryDragData(item.Id,out var data)){layoutKey="";return;}try{DragDrop.DoDragDrop(tile,data!,DragDropEffects.Link|DragDropEffects.Copy|DragDropEffects.Move);}catch(System.Runtime.InteropServices.COMException){layoutKey="";}catch(InvalidOperationException){layoutKey="";}};tile.MouseEnter+=(_,_)=>tile.Background=new SolidColorBrush(Color.FromArgb(25,255,255,255));tile.MouseLeave+=(_,_)=>tile.Background=Brushes.Transparent;tile.PreviewMouseRightButtonUp+=(_,e)=>{e.Handled=true;DesktopShellMenu.Show(item.Id,new WindowInteropHelper(window!).Handle);};icons.Children.Add(tile);}
 internal static bool TryDragData(string id,out DataObject? data){data=null;try{if(!ShellItems.Exists(id))return false;data=ShellItems.CreateData([id]);return true;}catch(IOException){return false;}catch(COMException){return false;}catch(ArgumentException){return false;}}
 static void Open(string id){try{if(ShellItems.IsVirtual(id))System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe",id.StartsWith("shell:",StringComparison.OrdinalIgnoreCase)?id:"shell:"+id){UseShellExecute=true});else System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(id){UseShellExecute=true});}catch(Exception ex){MessageBox.Show(ex.Message,"无法打开入口");}}
 void Stop(){window?.Close();window=null;icons.Children.Clear();layoutKey="";RestoreNative();listHandle=IntPtr.Zero;}
 public void Dispose(){if(disposed)return;Stop();disposed=true;}
}
