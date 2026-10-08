using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace DesktopBox;
internal static class UiTest {
 internal static void Run(Program app){
  app.ShutdownMode=ShutdownMode.OnExplicitShutdown;
  var root=Path.Combine(Path.GetTempPath(),"DesktopBox-UI-"+Guid.NewGuid());var folder=Path.Combine(root,"mapped");Directory.CreateDirectory(folder);
  File.WriteAllText(Path.Combine(folder,"会议记录.txt"),"test");Directory.CreateDirectory(Path.Combine(folder,"设计资料"));
  Store.PathName=Path.Combine(root,"state","ui-settings.json");
  var config=new BoxConfig{Name="工作资料",Folder=folder,Width=480,Height=430};
  var desktopFile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"DesktopBox-Lifecycle-"+Guid.NewGuid()+".txt");
  File.WriteAllText(desktopFile,"unchanged");Thread.Sleep(2000);
  var original=DesktopIcons.Inspect(desktopFile);var virtualConfig=new BoxConfig{Name="普通测试",Locked=true,Entries=[desktopFile]};
  app.Settings.Boxes.Add(config);app.Settings.Boxes.Add(virtualConfig);
  BoxWindow? virtualWindow=null,window=null;
  try {virtualWindow=new BoxWindow(app,virtualConfig);virtualWindow.Show();Assert(virtualWindow.ResizeMode==ResizeMode.NoResize,"Lock not applied");window=new BoxWindow(app,config);window.Show();}
  catch(Exception ex){Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),ex.ToString());app.Shutdown(1);return;}
  var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
  timer.Tick+=(_,_)=>{timer.Stop();try{
   window!.UpdateLayout();var origin=window.HeaderSurface.TranslatePoint(new Point(0,0),window);Assert(origin.X<=1&&origin.Y<=1,"Header does not reach top edge");Assert(window.HeaderSurface.ActualWidth>=window.ActualWidth-20,"Header does not span full client width: "+window.HeaderSurface.ActualWidth+" / "+window.ActualWidth);
   window.BeginRename();Assert(window.EditingName&&window.NameEditor.IsKeyboardFocused,"Inline editor did not receive focus");window.NameEditor.Text="已改名";
   window.NameEditor.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(window),Environment.TickCount,Key.Enter){RoutedEvent=Keyboard.KeyDownEvent});
   Assert(!window.EditingName&&config.Name=="已改名"&&window.Title=="已改名","Enter did not commit rename");
   window.BeginRename();window.NameEditor.Text="取消修改";window.NameEditor.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(window),Environment.TickCount,Key.Escape){RoutedEvent=Keyboard.KeyDownEvent});Assert(config.Name=="已改名"&&!window.EditingName,"Escape did not cancel rename");
   window.BeginRename();window.NameEditor.Text=" ";window.FinishRename(true);Assert(config.Name=="已改名","Empty name overwrote title");
   window.TransparencySlider.Value=75;Assert(config.Transparency==75&&((SolidColorBrush)window.Background).Color.A==64,"Transparency did not change background");Assert(window.Opacity==1,"Transparency faded foreground");
   var remembered=Store.Load().Boxes.First(x=>x.Id==config.Id);Assert(remembered.Name=="已改名"&&remembered.Transparency==75,"Appearance or rename not persisted");
   window.BeginRename();window.NameEditor.Text="工作资料";window.FinishRename(true);window.TransparencySlider.Value=40;
   foreach(var view in new[]{"图标","列表","详细信息"}){config.View=view;window.Refresh();window.UpdateLayout();var bmp=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);bmp.Render(window);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bmp));using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"ui-"+view+".png"));png.Save(output);}
   window.DisposeAndClose();window=null;virtualWindow!.DisposeAndClose();virtualWindow=null;
   var restored=DesktopIcons.Inspect(desktopFile);Assert(restored.X==original.X&&restored.Y==original.Y&&File.ReadAllText(desktopFile)=="unchanged","Window lifecycle restoration failed");
   Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),"PASS: full-width top drag surface hit area; inline rename focus/Enter/Escape/empty-name; transparency background-only change and persistence; three views rendered; position lock; ordinary close restores exact icon position and file; temporary files removed");app.Shutdown();
  }catch(Exception ex){Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),ex.ToString());app.Shutdown(1);}};timer.Start();
  void Cleanup(){window?.DisposeAndClose();virtualWindow?.DisposeAndClose();DesktopIcons.Recover();File.Delete(desktopFile);if(Directory.Exists(root))Directory.Delete(root,true);}
 }
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
}

