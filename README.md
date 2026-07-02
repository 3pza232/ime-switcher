# ime-switcher

Windows 输入法切换守护进程。通过 stdin 通信，零轮询、零进程创建开销。

## 用法

```bash
# 启动守护进程
ime-switcher.exe

# 在另一个终端或脚本中发送指令
echo en-US | ime-switcher.exe
echo zh-CN | ime-switcher.exe
```

## stdin 协议

| 输入 | 输出 (stderr) | 说明 |
|------|-------------|------|
| `zh-CN` | `ok` / `dup` / `fail` | 切换，dup 表示已在目标布局 |
| `en-US` | `ok` / `dup` / `fail` | 不限语言，只要是 Windows 已安装的键盘布局 |
| `status` | `zh-CN (Chinese (China))` | 当前布局 |
| `quit` | — | 退出 |

## 语言支持

不限语言。基于 `CultureInfo.LCID` + `GetKeyboardLayoutList` 运行时匹配，用户在 Windows 设置中安装了哪种键盘布局就支持哪种。

## 编译

```bash
dotnet publish -c Release
```

产物在 `bin/Release/net9.0/win-x64/publish/ime-switcher.exe`，单文件自包含，不依赖 .NET 运行时。
