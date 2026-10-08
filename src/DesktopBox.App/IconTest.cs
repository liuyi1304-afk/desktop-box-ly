using System.IO;
namespace DesktopBox;
internal static class IconTest {
 internal static int Run(){var path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"DesktopBox-验证-"+Guid.NewGuid().ToString("N")+".txt");var log=new List<string>();try{File.WriteAllText(path,"desktop icon test");var attr=File.GetAttributes(path);Thread.Sleep(2000);DesktopIcons.Hide(path);log.Add("Hide verified offscreen");if(!File.Exists(path)||File.ReadAllText(path)!="desktop icon test"||File.GetAttributes(path)!=attr)throw new Exception("File altered");log.Add("File path/content/attributes unchanged");DesktopIcons.Restore(path);log.Add("Restore succeeded");File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"icon-test.txt"),log);return 0;}catch(Exception ex){log.Add(ex.ToString());File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"icon-test.txt"),log);return 1;}finally{try{DesktopIcons.Restore(path);}catch{}File.Delete(path);}}
}
