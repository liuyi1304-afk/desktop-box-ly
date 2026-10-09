using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
namespace DesktopBox;
internal static class DesktopShellMenu {
 const uint TpmRet=0x0100,TpmRight=0x0002;
 [UnmanagedFunctionPointer(CallingConvention.StdCall)] delegate int GetUiObject(IntPtr self,IntPtr owner,uint count,[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=2)]IntPtr[] children,ref Guid iid,IntPtr reserved,out IntPtr result);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)] delegate int QueryMenu(IntPtr self,IntPtr menu,uint index,uint first,uint last,uint flags);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)] delegate int Invoke(IntPtr self,ref InvokeInfo info);
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct InvokeInfo {public uint Size,Mask;public IntPtr Owner,Verb,Parameters,Directory;public int Show;public uint HotKey;public IntPtr Icon;}
 [DllImport("shell32.dll",PreserveSig=true)]static extern int SHBindToParent(IntPtr pidl,ref Guid iid,out IntPtr parent,out IntPtr child);
 [DllImport("user32.dll")]static extern IntPtr CreatePopupMenu();
 [DllImport("user32.dll")]static extern bool DestroyMenu(IntPtr menu);
 [DllImport("user32.dll")]static extern uint TrackPopupMenuEx(IntPtr menu,uint flags,int x,int y,IntPtr owner,IntPtr parameters);
 [DllImport("user32.dll")]static extern bool GetCursorPos(out DesktopIcons.Point point);
 [DllImport("user32.dll")]static extern bool PostMessage(IntPtr hwnd,uint message,IntPtr wparam,IntPtr lparam);
 static T Method<T>(IntPtr ptr,int slot)where T:Delegate=>Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ptr),slot*IntPtr.Size));
 internal static void Show(string parsingName,IntPtr owner){IntPtr pidl=IntPtr.Zero,parent=IntPtr.Zero,context=IntPtr.Zero,menu=IntPtr.Zero;try{if(ShellItems.SHParseDisplayName(parsingName,IntPtr.Zero,out pidl,0,out _)<0)return;var folderIid=new Guid("000214E6-0000-0000-C000-000000000046");if(SHBindToParent(pidl,ref folderIid,out parent,out var child)<0)return;var menuIid=new Guid("000214E4-0000-0000-C000-000000000046");var get=Method<GetUiObject>(parent,10);if(get(parent,owner,1,[child],ref menuIid,IntPtr.Zero,out context)<0||context==IntPtr.Zero)return;menu=CreatePopupMenu();if(menu==IntPtr.Zero)return;var query=Method<QueryMenu>(context,3);if(query(context,menu,0,1,0x7FFF,0)<0)return;if(!GetCursorPos(out var point))return;var selected=TrackPopupMenuEx(menu,TpmRet|TpmRight,point.X,point.Y,owner,IntPtr.Zero);if(selected>=1){var info=new InvokeInfo{Size=(uint)Marshal.SizeOf<InvokeInfo>(),Owner=owner,Verb=new IntPtr(selected-1),Show=1};Method<Invoke>(context,4)(context,ref info);}PostMessage(owner,0,IntPtr.Zero,IntPtr.Zero);}catch(COMException){ }catch(InvalidOperationException){ }finally{if(menu!=IntPtr.Zero)DestroyMenu(menu);if(context!=IntPtr.Zero)Marshal.Release(context);if(parent!=IntPtr.Zero)Marshal.Release(parent);if(pidl!=IntPtr.Zero)Marshal.FreeCoTaskMem(pidl);}}
}
