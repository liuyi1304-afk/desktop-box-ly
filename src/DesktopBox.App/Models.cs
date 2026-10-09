using System.IO;
using System.Text.Json;
namespace DesktopBox;
public sealed class BoxConfig {
 public Guid Id {get;set;}=Guid.NewGuid(); public string Name {get;set;}="新盒子";
 public string? Folder {get;set;} public List<string> Entries {get;set;}=[];
 public double X {get;set;}=80; public double Y {get;set;}=100; public double Width {get;set;}=400; public double Height {get;set;}=440;
 public bool Locked {get;set;} public string View {get;set;}="图标";
}
public sealed class Settings {public int BackgroundIntervalMs {get;set;}=200;public int FileIntervalMs {get;set;}=2000;public double Frost {get;set;}=0; public double Transparency {get;set;}=80; public List<BoxConfig> Boxes {get;set;}=[]; public uint Modifiers {get;set;}=3; public uint Key {get;set;}=0x42;}
public static class Store {
 public static string PathName=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DesktopBoxLY","v1-settings.json");
 public static Settings Load(){ if(!File.Exists(PathName)) return new(); var settings=JsonSerializer.Deserialize<Settings>(File.ReadAllText(PathName)) ?? throw new InvalidDataException("设置为空");settings.BackgroundIntervalMs=Math.Clamp(settings.BackgroundIntervalMs,50,5000);settings.FileIntervalMs=Math.Clamp(settings.FileIntervalMs,250,60000);return settings; }
 public static void Save(Settings settings){Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);var temp=PathName+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(settings,new JsonSerializerOptions{WriteIndented=true}));File.Move(temp,PathName,true);}
}
public static class FileActions {
 public static string CopyInto(string source,string folder){
  if(!Directory.Exists(folder))throw new DirectoryNotFoundException("映射路径失效，请重新绑定。");
  source=Path.GetFullPath(source);folder=Path.GetFullPath(folder);var name=Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar));var target=Path.Combine(folder,name);
  if(string.Equals(source,target,StringComparison.OrdinalIgnoreCase))return target;
  if(File.Exists(target)||Directory.Exists(target)){var stem=Path.GetFileNameWithoutExtension(name);var extension=Path.GetExtension(name);for(var i=1;i<int.MaxValue;i++){target=Path.Combine(folder,$"{stem} - 副本{i}{extension}");if(!File.Exists(target)&&!Directory.Exists(target))break;}}
  if(Directory.Exists(source))CopyDirectory(source,target);else File.Copy(source,target,false);return target;
 }
 static void CopyDirectory(string source,string target){Directory.CreateDirectory(target);foreach(var file in Directory.EnumerateFiles(source))File.Copy(file,Path.Combine(target,Path.GetFileName(file)));foreach(var directory in Directory.EnumerateDirectories(source))CopyDirectory(directory,Path.Combine(target,Path.GetFileName(directory)));}
 public static string MoveInto(string source,string folder){
  if(!Directory.Exists(folder)) throw new DirectoryNotFoundException("映射路径失效，请重新绑定。");
  source=Path.GetFullPath(source);folder=Path.GetFullPath(folder);var target=Path.Combine(folder,Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar)));
  if(string.Equals(source,target,StringComparison.OrdinalIgnoreCase)) return target;
  if(File.Exists(target)||Directory.Exists(target)) throw new IOException("同名冲突，未移动也未覆盖："+Path.GetFileName(target));
  if(Directory.Exists(source)){if(folder.StartsWith(source.TrimEnd('\\')+"\\",StringComparison.OrdinalIgnoreCase)) throw new IOException("不能将文件夹移入自身。");Directory.Move(source,target);}
  else File.Move(source,target,false);
  return target;
 }
}
