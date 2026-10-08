using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
namespace DesktopBox;
internal static class MenuGlass {
 internal static void Attach(ContextMenu menu,Func<Settings> settings){
  var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(200)};IntPtr handle=IntPtr.Zero;Image? image=null;
  void Update(){if(!menu.IsOpen)return;var s=settings();timer.Interval=TimeSpan.FromMilliseconds(Math.Clamp(s.BackgroundIntervalMs,50,5000));var amount=Math.Clamp(s.Transparency,0,95);menu.Background=new SolidColorBrush(Color.FromArgb((byte)Math.Round(255*(1-amount/100)),28,35,44));menu.Clip=new RectangleGeometry(new Rect(0,0,menu.ActualWidth,menu.ActualHeight),8,8);image??=menu.Template.FindName("Frost",menu) as Image;if(image==null)return;var frost=Math.Clamp(s.Frost,0,100);image.Opacity=amount/100;image.Effect=new BlurEffect{Radius=frost*0.3,RenderingBias=RenderingBias.Quality};if(frost<=0){image.Source=null;BackgroundSampler.SetExcluded(handle,false);return;}if(!BackgroundSampler.SetExcluded(handle,true)){image.Source=null;return;}try{image.Source=BackgroundSampler.Read(handle);}catch{image.Source=null;}}
  menu.Opened+=(_,_)=>{menu.UpdateLayout();handle=(PresentationSource.FromVisual(menu) as HwndSource)?.Handle??IntPtr.Zero;Update();timer.Start();};timer.Tick+=(_,_)=>Update();menu.Closed+=(_,_)=>{timer.Stop();if(handle!=IntPtr.Zero)BackgroundSampler.SetExcluded(handle,false);if(image!=null)image.Source=null;image=null;handle=IntPtr.Zero;};
 }
}
