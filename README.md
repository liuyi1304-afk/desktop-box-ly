# Desktop Box LY

Windows 11 桌面收纳盒第一版，当前版本 **v0.1.3 实验版**。

直接运行 `releases/v0.1.3/DesktopBox.App.exe`，保留同目录配套文件。无需另装 .NET 或 GitHub Desktop。

- 普通盒子：虚拟文件入口，原文件位置与属性不变；指定桌面图标通过屏幕外位置实验性隐藏。
- 映射盒子：显示真实文件夹并同步刷新；拖入移动真实文件，同名冲突拒绝覆盖。
- 图标 / 列表 / 详细信息视图、顶部拖动、单击标题改名、全局透明度、磨砂浓度、位置锁定、缩放、布局记忆、托盘与可修改的全局快捷键。
- 删除盒子保留文件；移出普通盒子或退出恢复原图标，包含异常终止恢复助手。

单图标隐藏需关闭自动排列，受 Explorer 和系统变化限制。快捷键置顶不能覆盖安全桌面等所有画面。完整操作说明与验证范围见 [使用说明](releases/v0.1.3/使用说明.md) 和 [验证记录](releases/v0.1.3/验证记录.md)。

运行版源码：`src/DesktopBox.App`（C# / WPF）；应用图标沿用“装盘&装柜”。玻璃为真实逐像素透明；磨砂默认关闭，开启会影响系统截图和录屏，详见使用说明。原 WinUI 草架保留在 `src/DesktopBoxLY`。项目记忆见 `项目记忆.md`。

## 构建

```powershell
.\.build\dotnet\dotnet.exe publish src\DesktopBox.App -c Release -r win-x64 --self-contained true -o output\local-build
```

本地 SDK 不加入 Git；其他开发机可使用 .NET 10 SDK。正式发布包不覆盖历史版本。软件配置只在本机用户目录保存。



