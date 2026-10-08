using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Forms=System.Windows.Forms;
namespace DesktopBox;
public sealed class Program : System.Windows.Application {
 internal Settings Settings=new(); internal new List<BoxWindow> Windows=[]; Forms.NotifyIcon? tray; Manager? manager; bool hidden; bool ready; System.Threading.Mutex? mutex;
 [STAThread] public static void Main(string[] args){if(args.Contains("--restore-icons")){try{DesktopIcons.Recover();MessageBox.Show("已恢复有记录的原桌面图标。");}catch(Exception ex){MessageBox.Show("恢复失败，记录已保留："+ex.Message);}return;}if(args.Length==2&&args[0]=="--icon-hold"){DesktopIcons.Hide(args[1]);File.WriteAllText(args[1]+".ready","ready");Thread.Sleep(Timeout.Infinite);return;}if(args.Contains("--crash-test")){Environment.Exit(CrashTest.Run());return;}if(args.Contains("--icon-test")){Environment.Exit(IconTest.Run());return;} if(args.Length==2&&args[0]=="--watch"){try{try{System.Diagnostics.Process.GetProcessById(int.Parse(args[1])).WaitForExit();}catch(ArgumentException){}DesktopIcons.Recover();}catch(Exception ex){File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"recovery-error.txt"),ex.ToString());}return;} if(args.Contains("--self-test")){Environment.Exit(SelfTest.Run());return;} var app=new Program();app.Run();}
 protected override void OnStartup(StartupEventArgs e){base.OnStartup(e);if(e.Args.Contains("--ui-test")){UiTest.Run(this);return;}mutex=new(true,"Local\\DesktopBoxLY-v1",out var first);if(!first){Shutdown();return;}ShutdownMode=ShutdownMode.OnExplicitShutdown;
 try{Settings=Store.Load();}catch(Exception ex){MessageBox.Show("设置无法读取，已保留原文件。程序停止以避免覆盖。\n"+ex.Message);Shutdown();return;}
 try{DesktopIcons.Recover();}catch(Exception ex){MessageBox.Show("桌面图标恢复失败，恢复记录已保留："+ex.Message);Shutdown();return;}
 ready=true;StartRecoveryHelper();foreach(var c in Settings.Boxes) Open(c);
 manager=new Manager(this); manager.Show(); tray=new Forms.NotifyIcon{Icon=System.Drawing.SystemIcons.Application,Text="桌面盒子",Visible=true};var menu=new Forms.ContextMenuStrip();menu.Items.Add("管理盒子",null,(_,_)=>Dispatcher.Invoke(()=>{manager.Show();manager.Activate();}));menu.Items.Add("显示 / 隐藏",null,(_,_)=>Dispatcher.Invoke(Toggle));menu.Items.Add("退出",null,(_,_)=>Dispatcher.Invoke(Shutdown));tray.ContextMenuStrip=menu;tray.DoubleClick+=(_,_)=>Dispatcher.Invoke(()=>{manager.Show();manager.Activate();});}
 void StartRecoveryHelper(){var exe=Environment.ProcessPath!;var watchArgs="--watch "+Environment.ProcessId; if(Path.GetFileNameWithoutExtension(exe).Equals("dotnet",StringComparison.OrdinalIgnoreCase)) watchArgs="\""+typeof(Program).Assembly.Location+"\" "+watchArgs;System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe,watchArgs){UseShellExecute=false,CreateNoWindow=true});}
 internal void Open(BoxConfig c){var w=new BoxWindow(this,c);Windows.Add(w);w.Show();}
 internal void Save(){try{Store.Save(Settings);}catch(Exception ex){MessageBox.Show("保存失败："+ex.Message);}}
 internal void Toggle(){hidden=!hidden;foreach(var w in Windows){if(hidden)w.Hide();else{w.Show();w.Topmost=true;w.Activate();}}}
 internal void Delete(BoxWindow w){if(MessageBox.Show("删除盒子只移除配置，所有真实文件保留。继续？","删除盒子",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;Settings.Boxes.Remove(w.Config);Windows.Remove(w);w.DisposeAndClose();Save();manager?.Refresh();}
 protected override void OnExit(ExitEventArgs e){foreach(var w in Windows.ToArray())w.DisposeAndClose();if(ready)Save();tray?.Dispose();mutex?.Dispose();base.OnExit(e);}
}
internal sealed class Manager:Window {
 readonly Program app; readonly StackPanel list=new(); IntPtr handle;
 internal Manager(Program app){this.app=app;Title="桌面盒子 · 管理";Width=480;Height=520;Background=new SolidColorBrush(Color.FromRgb(23,32,47));Foreground=Brushes.White;
 var panel=new StackPanel{Margin=new Thickness(22)};Content=new ScrollViewer{Content=panel};panel.Children.Add(new TextBlock{Text="桌面盒子",FontSize=26});panel.Children.Add(new TextBlock{Text="普通盒子保留文件位置；映射盒子拖入会移动文件。\n桌面图标隐藏为实验功能，需关闭自动排列。",Margin=new Thickness(0,12,0,12),TextWrapping=TextWrapping.Wrap});
 panel.Children.Add(Button("＋ 普通盒子",()=>Create(false)));panel.Children.Add(Button("＋ 文件夹映射",()=>Create(true)));panel.Children.Add(Button("显示 / 隐藏所有盒子",app.Toggle));panel.Children.Add(Button("设置全局快捷键",ChangeHotkey));panel.Children.Add(new TextBlock{Text="默认 Ctrl + Alt + B；唤出置顶。\n安全桌面、锁屏及部分独占全屏不可覆盖。",Margin=new Thickness(0,10,0,12)});panel.Children.Add(list);panel.Children.Add(Button("退出软件",app.Shutdown));
 SourceInitialized+=(_,_)=>{handle=new WindowInteropHelper(this).Handle;HwndSource.FromHwnd(handle).AddHook(Hook);if(!Native.RegisterHotKey(handle,1,app.Settings.Modifiers|0x4000,app.Settings.Key))MessageBox.Show("快捷键已被占用，请在设置中修改。");};Closing+=(_,e)=>{e.Cancel=true;Hide();};Refresh();}
 IntPtr Hook(IntPtr h,int msg,IntPtr w,IntPtr l,ref bool handled){if(msg==0x312){app.Toggle();handled=true;}return IntPtr.Zero;}
 internal static System.Windows.Controls.Button Button(string text,Action action){var b=new System.Windows.Controls.Button{Content=text,Margin=new Thickness(0,4,0,4),Padding=new Thickness(10,7,10,7),Foreground=Brushes.DarkSlateGray,Background=new SolidColorBrush(Color.FromRgb(225,235,245)),HorizontalContentAlignment=HorizontalAlignment.Left};b.Click+=(_,_)=>action();return b;}
 void Create(bool map){var folder=map?PickFolder():null;if(map&&folder==null)return;var c=new BoxConfig{Name=map?Path.GetFileName(folder!):"普通盒子",Folder=folder,X=80+app.Windows.Count*30,Y=100+app.Windows.Count*30};app.Settings.Boxes.Add(c);app.Open(c);app.Save();Refresh();}
 internal void Refresh(){list.Children.Clear();foreach(var w in app.Windows.ToArray()){var b=Button(w.Config.Name+(w.Config.Folder==null?" · 普通":" · 映射"),()=>{w.Show();w.Activate();});list.Children.Add(b);}}
 internal static string? PickFolder(){using var d=new Forms.FolderBrowserDialog{Description="选择映射文件夹",UseDescriptionForTitle=true};return d.ShowDialog()==Forms.DialogResult.OK?d.SelectedPath:null;}
 internal static string? Ask(Window owner,string title,string initial){var d=new Window{Owner=owner,Title=title,Width=360,Height=180,WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize};var p=new StackPanel{Margin=new Thickness(15)};var t=new TextBox{Text=initial,Margin=new Thickness(0,8,0,12)};p.Children.Add(t);p.Children.Add(Button("确定",()=>{d.DialogResult=true;}));d.Content=p;return d.ShowDialog()==true?t.Text.Trim():null;}
 void ChangeHotkey(){var s=Ask(this,"快捷键，例如 Ctrl+Alt+B","Ctrl+Alt+B");if(s==null)return;uint mods=0,key=0;foreach(var part in s.Split('+')){var p=part.Trim().ToUpperInvariant();if(p=="CTRL")mods|=2;else if(p=="ALT")mods|=1;else if(p=="SHIFT")mods|=4;else if(p=="WIN")mods|=8;else if(p.Length==1&&char.IsAsciiLetterOrDigit(p[0]))key=p[0];else{MessageBox.Show("使用 Ctrl/Alt/Shift/Win 加一个字母或数字。");return;}}if(mods==0||key==0){MessageBox.Show("需要修饰键和字母/数字。");return;}Native.UnregisterHotKey(handle,1);if(!Native.RegisterHotKey(handle,1,mods|0x4000,key)){Native.RegisterHotKey(handle,1,app.Settings.Modifiers|0x4000,app.Settings.Key);MessageBox.Show("快捷键被占用，保留原快捷键。");return;}app.Settings.Modifiers=mods;app.Settings.Key=key;app.Save();}
}





