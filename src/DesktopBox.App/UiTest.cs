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
  BoxWindow? virtualWindow=null,window=null;Manager? manager=null;
  try {virtualWindow=new BoxWindow(app,virtualConfig);virtualWindow.Show();Assert(virtualWindow.ResizeMode==ResizeMode.NoResize,"Lock not applied");window=new BoxWindow(app,config);window.Show();app.Windows.Add(window);app.Windows.Add(virtualWindow);manager=new Manager(app,false);manager.Show();}
  catch(Exception ex){Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),ex.ToString());app.Shutdown(1);return;}
  var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
  timer.Tick+=(_,_)=>{timer.Stop();try{
   window!.UpdateLayout();var origin=window.HeaderSurface.TranslatePoint(new Point(0,0),window);Assert(origin.X<=1&&origin.Y<=1,"Header does not reach top edge");Assert(window.HeaderSurface.ActualWidth>=window.ActualWidth-20,"Header does not span full client width: "+window.HeaderSurface.ActualWidth+" / "+window.ActualWidth);
   window.BeginRename();Assert(window.EditingName&&window.NameEditor.IsKeyboardFocused,"Inline editor did not receive focus");window.NameEditor.Text="已改名";
   window.NameEditor.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(window),Environment.TickCount,Key.Enter){RoutedEvent=Keyboard.KeyDownEvent});
   Assert(!window.EditingName&&config.Name=="已改名"&&window.Title=="已改名","Enter did not commit rename");
   window.BeginRename();window.NameEditor.Text="取消修改";window.NameEditor.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(window),Environment.TickCount,Key.Escape){RoutedEvent=Keyboard.KeyDownEvent});Assert(config.Name=="已改名"&&!window.EditingName,"Escape did not cancel rename");
   window.BeginRename();window.NameEditor.Text=" ";window.FinishRename(true);Assert(config.Name=="已改名","Empty name overwrote title");
   manager!.TransparencySlider.Value=75;Assert(app.Settings.Transparency==75&&virtualWindow!.TintAlpha==64&&window.TintAlpha==64,"Transparency did not change background");var added=new BoxWindow(app,new BoxConfig());Assert(added.TintAlpha==64,"New box did not inherit global transparency");added.DisposeAndClose();Assert(window.Opacity==1&&window.AllowsTransparency,"Transparency did not enable per-pixel background");Assert(virtualWindow.ResizeHandle.Visibility==Visibility.Collapsed,"Locked resize handle is visible");var beforeWidth=window.Width;window.ResizeHandle.RaiseEvent(new System.Windows.Controls.Primitives.DragDeltaEventArgs(20,15){RoutedEvent=System.Windows.Controls.Primitives.Thumb.DragDeltaEvent});Assert(window.Width==beforeWidth+20,"Resize handle did not resize");
   var remembered=Store.Load().Boxes.First(x=>x.Id==config.Id);Assert(remembered.Name=="已改名"&&Store.Load().Transparency==75,"Appearance or rename not persisted");
   window.BeginRename();window.NameEditor.Text="工作资料";window.FinishRename(true);manager!.TransparencySlider.Value=80;manager.Height=640;manager.UpdateLayout();var managerImage=new RenderTargetBitmap((int)manager.ActualWidth,(int)manager.ActualHeight,96,96,PixelFormats.Pbgra32);managerImage.Render(manager);var managerEncoder=new PngBitmapEncoder();managerEncoder.Frames.Add(BitmapFrame.Create(managerImage));using(var managerOutput=File.Create(Path.Combine(AppContext.BaseDirectory,"ui-管理页面.png")))managerEncoder.Save(managerOutput);
   foreach(var view in new[]{"图标","列表","详细信息"}){config.View=view;window.Refresh();window.UpdateLayout();var bmp=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);bmp.Render(window);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bmp));using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"ui-"+view+".png"));png.Save(output);}
   window.DisposeAndClose();app.Windows.Remove(window);window=null;virtualWindow!.DisposeAndClose();app.Windows.Remove(virtualWindow);virtualWindow=null;manager!.Hide();
   var restored=DesktopIcons.Inspect(desktopFile);Assert(restored.X==original.X&&restored.Y==original.Y&&File.ReadAllText(desktopFile)=="unchanged","Window lifecycle restoration failed");
   Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),"PASS: full-width top drag surface hit area; inline rename focus/Enter/Escape/empty-name; manager global transparency updates both boxes and new boxes and persistence; layered glass and resize handle; three views rendered; position lock; ordinary close restores exact icon position and file; temporary files removed");app.Shutdown();
  }catch(Exception ex){Cleanup();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-test.txt"),ex.ToString());app.Shutdown(1);}};timer.Start();
  void Cleanup(){window?.DisposeAndClose();virtualWindow?.DisposeAndClose();app.Windows.Clear();manager?.Hide();DesktopIcons.Recover();File.Delete(desktopFile);if(Directory.Exists(root))Directory.Delete(root,true);}
 }
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
}




