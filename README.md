# Desktop Box LY

Windows 11 桌面盒子管理器，原仓库中的 MVP 原型说明为：C# + WinUI 3，可配置半透明桌面盒子、映射文件夹并进行文件操作。

目前 GitHub 初始提交里只有仓库说明和许可证，没有可恢复的应用源码或安装包。本地重建按项目分目录工作，规格草案见 [docs/重建规格草案.md](docs/重建规格草案.md)。

## Proposed first version

- Multiple movable, resizable translucent boxes.
- Each box maps to one local folder and remembers its layout.
- Browse, open, refresh, and reveal files in Explorer.
- Keep file-changing actions out until their behavior is defined.

## Development

The repository specifies C# and WinUI 3. The current machine does not yet have the .NET / Windows App SDK build tools, so the app cannot be compiled here until the project-local toolchain is prepared.
