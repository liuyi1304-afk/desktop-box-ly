# 桌面盒子

Windows 11 桌面收纳盒，正式版 **v1.0.24**。

下载：[Windows x64 正式版](https://github.com/liuyi1304-afk/desktop-box-ly/releases/latest/download/DesktopBox-1.0.24-win-x64.zip)，完整解压后运行 `DesktopBox.App.exe`，无需另装 .NET。

- 普通盒子：虚拟文件入口，原文件位置与属性不变；桌面替代层实验性隐藏已收纳项，退出恢复系统图标层。
- 映射盒子：显示真实文件夹并同步刷新；拖入移动真实文件，同名冲突拒绝覆盖。
- 图标 / 列表 / 详细信息视图、顶部拖动、单击标题改名、全局透明度、磨砂浓度、位置锁定、缩放、布局记忆、托盘与可修改的全局快捷键。
- 删除盒子保留文件；移出普通盒子或退出恢复原图标，包含异常终止恢复助手。

桌面图标层使用 Windows Shell 菜单，依赖 Explorer 和系统 Shell 扩展。快捷键置顶不能覆盖安全桌面等所有画面。完整操作说明见包内正式版说明。

运行版源码：`src/DesktopBox.App`（C# / WPF）；应用图标沿用“装盘&装柜”。玻璃为真实逐像素透明；磨砂默认关闭，开启会影响系统截图和录屏，详见使用说明。原 WinUI 草架保留在 `src/DesktopBoxLY`。项目记忆见 `项目记忆.md`。

## 构建

```powershell
.\.build\dotnet\dotnet.exe publish src\DesktopBox.App -c Release -r win-x64 --self-contained true -o output\local-build
```

本地 SDK 不加入 Git；其他开发机可使用 .NET 10 SDK。正式发布包不覆盖历史版本。软件配置只在本机用户目录保存。
