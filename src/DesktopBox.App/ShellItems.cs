using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using ComData=System.Runtime.InteropServices.ComTypes.IDataObject;
using ComTypes=System.Runtime.InteropServices.ComTypes;
namespace DesktopBox;
internal static class ShellItems {
 internal const string DragFormat="Shell IDList Array";
 const string FileGroupDescriptorW="FileGroupDescriptorW",FileContents="FileContents";
 const int MaxVirtualFiles=4096;
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct FileDescriptorW{public uint Flags;public Guid ClassId;public int SizeX,SizeY,PointX,PointY;public uint FileAttributes;public ComTypes.FILETIME CreationTime,AccessTime,WriteTime;public uint FileSizeHigh,FileSizeLow;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string? FileName;}
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern uint RegisterClipboardFormat(string format);
 [DllImport("ole32.dll")]static extern void ReleaseStgMedium(ref ComTypes.STGMEDIUM medium);
 [DllImport("kernel32.dll",SetLastError=true)]static extern IntPtr GlobalLock(IntPtr memory);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool GlobalUnlock(IntPtr memory);
 [DllImport("kernel32.dll",SetLastError=true)]static extern UIntPtr GlobalSize(IntPtr memory);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int GetName(IntPtr self,uint kind,out IntPtr text);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int Count(IntPtr self,out uint count);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int Item(IntPtr self,uint index,out IntPtr item);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]internal static extern int SHParseDisplayName(string name,IntPtr context,out IntPtr pidl,uint attrs,out uint actual);
 [DllImport("shell32.dll")]static extern int SHGetNameFromIDList(IntPtr pidl,uint kind,out IntPtr text);
 [DllImport("shell32.dll")]static extern IntPtr ILCombine(IntPtr parent,IntPtr child);
 [DllImport("shell32.dll")]static extern int SHCreateShellItemArrayFromDataObject(ComData data,ref Guid iid,out IntPtr array);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern int SHCreateItemFromParsingName(string name,IntPtr context,ref Guid iid,out IntPtr item);
 static T Method<T>(IntPtr ptr,int slot)where T:Delegate=>Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ptr),slot*IntPtr.Size));
 internal static bool IsVirtual(string path)=>path.StartsWith("::",StringComparison.Ordinal)||path.StartsWith("shell:",StringComparison.OrdinalIgnoreCase);
 internal static string NameOf(IntPtr item,uint kind){if(Method<GetName>(item,5)(item,kind,out var text)<0)return "";try{return Marshal.PtrToStringUni(text)??"";}finally{Marshal.FreeCoTaskMem(text);}}
 internal static string Identity(IntPtr item){var path=NameOf(item,0x80058000);return path.Length>0?path:NameOf(item,0x80028000);}
 static readonly Dictionary<string,string> names=new(StringComparer.OrdinalIgnoreCase);
 internal static string DisplayName(string path){if(!IsVirtual(path))return Path.GetFileName(path);if(names.TryGetValue(path,out var cached))return cached;var iid=new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");if(SHCreateItemFromParsingName(path,IntPtr.Zero,ref iid,out var item)<0)return "系统入口";try{return names[path]=NameOf(item,0);}finally{Marshal.Release(item);}}
 internal static bool Exists(string path)=>IsVirtual(path)?SHExists(path):File.Exists(path)||Directory.Exists(path);
 static bool SHExists(string path){if(SHParseDisplayName(path,IntPtr.Zero,out var pidl,0,out _) <0)return false;Marshal.FreeCoTaskMem(pidl);return true;}
 internal static bool HasVirtualFiles(System.Windows.IDataObject data)=>data.GetDataPresent(FileGroupDescriptorW,false);
 internal static bool CanRead(System.Windows.IDataObject data)=>data.GetDataPresent(DragFormat,false)||data.GetDataPresent(DataFormats.FileDrop)||HasVirtualFiles(data);
 internal static DataObject CreateData(IReadOnlyList<string> paths){var pidls=new List<byte[]>();foreach(var path in paths){Marshal.ThrowExceptionForHR(SHParseDisplayName(path,IntPtr.Zero,out var ptr,0,out _));try{int size=0;while(true){int part=(ushort)Marshal.ReadInt16(ptr,size);if(part==0){size+=2;break;}if(part<2||size+part>1024*1024)throw new IOException("系统入口数据无效");size+=part;}var bytes=new byte[size];Marshal.Copy(ptr,bytes,0,size);pidls.Add(bytes);}finally{Marshal.FreeCoTaskMem(ptr);}}int header=4+(paths.Count+1)*4;using var buffer=new MemoryStream();using var writer=new BinaryWriter(buffer);writer.Write(paths.Count);writer.Write(header);int offset=header+2;foreach(var pidl in pidls){writer.Write(offset);offset+=pidl.Length;}writer.Write((ushort)0);foreach(var pidl in pidls)writer.Write(pidl);var data=new DataObject();data.SetData(DragFormat,new MemoryStream(buffer.ToArray()));if(paths.All(x=>!IsVirtual(x)))data.SetData(DataFormats.FileDrop,paths.ToArray());return data;}
 internal static IReadOnlyList<string> Read(System.Windows.IDataObject data){
  // Prefer concrete local paths when the source provides them. Some apps also advertise
  // virtual-file formats, which can trigger source-side warnings if we request their streams.
  if(data.GetDataPresent(DataFormats.FileDrop,false)&&data.GetData(DataFormats.FileDrop,false) is string[] droppedPaths&&droppedPaths.Length>0&&droppedPaths.All(p=>File.Exists(p)||Directory.Exists(p)))return droppedPaths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
  var virtualFiles=ReadVirtualFiles(data);if(virtualFiles!=null)return virtualFiles;
  // Prefer Shell identity over converted FileDrop data, retaining shortcuts rather than their targets.
  if(data.GetDataPresent(DragFormat,false)){
   if(data is ComData native){var iid=new Guid("B63EA76D-1F85-456F-A19C-48159EFA858B");if(SHCreateShellItemArrayFromDataObject(native,ref iid,out var array)>=0){try{Marshal.ThrowExceptionForHR(Method<Count>(array,7)(array,out var count));if(count>4096)throw new IOException("拖入项目过多");var result=new List<string>();for(uint i=0;i<count;i++){Marshal.ThrowExceptionForHR(Method<Item>(array,8)(array,i,out var item));try{var id=Identity(item);if(id.Length>0)result.Add(id);}finally{Marshal.Release(item);}}return result.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();}finally{Marshal.Release(array);}}}
   if(data.GetData(DragFormat,false) is Stream stream){long position=stream.CanSeek?stream.Position:0;try{if(stream.CanSeek)stream.Position=0;using var buffer=new MemoryStream();var chunk=new byte[4096];int n;while((n=stream.Read(chunk,0,chunk.Length))>0){buffer.Write(chunk,0,n);if(buffer.Length>16*1024*1024)throw new IOException("拖入数据过大");}return Parse(buffer.ToArray());}finally{if(stream.CanSeek)stream.Position=position;}}
  }
  return data.GetData(DataFormats.FileDrop) is string[] paths?paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray():[];
 }
 static IReadOnlyList<string>? ReadVirtualFiles(System.Windows.IDataObject data){
  if(!HasVirtualFiles(data))return null;
  if(data is not ComData native)throw new IOException("来源应用提供了虚拟文件，但无法读取文件数据。");
  var format=unchecked((short)RegisterClipboardFormat(FileGroupDescriptorW));if(format==0)throw new IOException("无法识别虚拟文件格式。");
  var descriptorRequest=new ComTypes.FORMATETC{cfFormat=format,dwAspect=ComTypes.DVASPECT.DVASPECT_CONTENT,lindex=-1,tymed=ComTypes.TYMED.TYMED_HGLOBAL};
  native.GetData(ref descriptorRequest,out var descriptorMedium);
  try{
   if(descriptorMedium.tymed!=ComTypes.TYMED.TYMED_HGLOBAL)throw new IOException("来源应用的文件列表格式不受支持。");
   var size=GlobalSize(descriptorMedium.unionmember).ToUInt64();if(size<4||size>16*1024*1024)throw new IOException("来源应用提供的文件列表无效。");
   var locked=GlobalLock(descriptorMedium.unionmember);if(locked==IntPtr.Zero)throw new IOException("无法读取来源应用的文件列表。");
   try{
    var count=unchecked((uint)Marshal.ReadInt32(locked));var descriptorSize=Marshal.SizeOf<FileDescriptorW>();if(count>MaxVirtualFiles||4L+(long)count*descriptorSize>(long)size)throw new IOException("来源应用提供的文件数量或列表长度无效。");
    var inbox=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DesktopBoxLY","Incoming");Directory.CreateDirectory(inbox);var result=new List<string>((int)count);
    for(var index=0;index<count;index++){
     var descriptor=Marshal.PtrToStructure<FileDescriptorW>(IntPtr.Add(locked,checked(4+(int)index*descriptorSize)));
     if((descriptor.FileAttributes&0x10)!=0)throw new IOException("暂不支持从此应用拖入文件夹。");
     var name=Path.GetFileName((descriptor.FileName??"").Replace('\\','/'));foreach(var invalid in Path.GetInvalidFileNameChars())name=name.Replace(invalid,'_');if(string.IsNullOrWhiteSpace(name))name="微信文件";
     var target=UniquePath(inbox,name);var contentRequest=new ComTypes.FORMATETC{cfFormat=unchecked((short)RegisterClipboardFormat(FileContents)),dwAspect=ComTypes.DVASPECT.DVASPECT_CONTENT,lindex=(int)index,tymed=ComTypes.TYMED.TYMED_ISTREAM|ComTypes.TYMED.TYMED_HGLOBAL};
     native.GetData(ref contentRequest,out var contentMedium);
     try{using(var output=new FileStream(target,FileMode.CreateNew,FileAccess.Write,FileShare.None)){if(contentMedium.tymed==ComTypes.TYMED.TYMED_ISTREAM)CopyStream(contentMedium.unionmember,output);else if(contentMedium.tymed==ComTypes.TYMED.TYMED_HGLOBAL)CopyGlobal(contentMedium.unionmember,output);else throw new IOException("来源应用的文件内容格式不受支持。");}result.Add(target);}
     catch{try{File.Delete(target);}catch{}throw;}
     finally{ReleaseStgMedium(ref contentMedium);}
    }
    return result;
   }finally{GlobalUnlock(descriptorMedium.unionmember);}
  }finally{ReleaseStgMedium(ref descriptorMedium);}
 }
 static string UniquePath(string directory,string name){var candidate=Path.Combine(directory,name);if(!File.Exists(candidate)&&!Directory.Exists(candidate))return candidate;var stem=Path.GetFileNameWithoutExtension(name);var ext=Path.GetExtension(name);for(var i=1;i<int.MaxValue;i++){candidate=Path.Combine(directory,$"{stem} ({i}){ext}");if(!File.Exists(candidate)&&!Directory.Exists(candidate))return candidate;}throw new IOException("无法为拖入文件分配保存名称。");}
 static void CopyGlobal(IntPtr handle,Stream destination){var size=GlobalSize(handle).ToUInt64();if(size>long.MaxValue)throw new IOException("拖入文件过大。");var pointer=GlobalLock(handle);if(pointer==IntPtr.Zero)throw new IOException("无法读取拖入的文件内容。");try{var buffer=new byte[64*1024];ulong offset=0;while(offset<size){var length=(int)Math.Min((ulong)buffer.Length,size-offset);Marshal.Copy(IntPtr.Add(pointer,checked((int)offset)),buffer,0,length);destination.Write(buffer,0,length);offset+=(uint)length;}}finally{GlobalUnlock(handle);}}
 static void CopyStream(IntPtr pointer,Stream destination){var stream=(ComTypes.IStream)Marshal.GetObjectForIUnknown(pointer);var readPointer=Marshal.AllocCoTaskMem(sizeof(int));var buffer=new byte[64*1024];try{while(true){Marshal.WriteInt32(readPointer,0);stream.Read(buffer,buffer.Length,readPointer);var read=Marshal.ReadInt32(readPointer);if(read<=0)break;destination.Write(buffer,0,read);}}finally{Marshal.FreeCoTaskMem(readPointer);if(Marshal.IsComObject(stream))Marshal.ReleaseComObject(stream);}}
 internal static IReadOnlyList<string> Parse(byte[] bytes){if(bytes.Length<8)throw new IOException("无效的系统入口拖入数据");uint count=BitConverter.ToUInt32(bytes,0);if(count>4096||4L+(count+1L)*4>bytes.Length)throw new IOException("无效的系统入口数量");int PidlOffset(int index){uint raw=BitConverter.ToUInt32(bytes,4+index*4);if(raw<4L+(count+1L)*4||raw>bytes.Length-2)throw new IOException("无效的系统入口偏移");int offset=(int)raw,cursor=offset;while(true){if(cursor>bytes.Length-2)throw new IOException("系统入口数据不完整");int size=BitConverter.ToUInt16(bytes,cursor);if(size==0)break;if(size<2||size>bytes.Length-cursor)throw new IOException("无效的系统入口长度");cursor+=size;}return offset;}
  int parentOffset=PidlOffset(0);var children=Enumerable.Range(1,(int)count).Select(PidlOffset).ToArray();var pinned=GCHandle.Alloc(bytes,GCHandleType.Pinned);try{var root=pinned.AddrOfPinnedObject();var result=new List<string>();foreach(var offset in children){var full=ILCombine(root+parentOffset,root+offset);if(full==IntPtr.Zero)throw new OutOfMemoryException();try{uint kind=0x80058000;if(SHGetNameFromIDList(full,kind,out var text)<0)Marshal.ThrowExceptionForHR(SHGetNameFromIDList(full,0x80028000,out text));try{var id=Marshal.PtrToStringUni(text);if(!string.IsNullOrEmpty(id))result.Add(id);}finally{Marshal.FreeCoTaskMem(text);}}finally{Marshal.FreeCoTaskMem(full);}}return result.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();}finally{pinned.Free();}}
}
