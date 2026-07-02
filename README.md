# ime-switcher

Windows 输入法切换工具。支持任意已安装键盘布局，基于 `PostMessage` / `ActivateKeyboardLayout`。

## 用法

```bash
# 直接切换（CLI 模式，供脚本或编辑器调用）
ime-switcher.exe zh-CN
ime-switcher.exe en-US
ime-switcher.exe ja-JP

# 后台进程（stdin 模式，供常驻使用）
ime-switcher.exe
```

## 语言支持

不限语言。基于 `CultureInfo.LCID` + `GetKeyboardLayoutList` 运行时匹配，Windows 已添加的任意键盘布局均可。常用：

- `en-US` — 英语（美国）
- `zh-CN` — 中文（简体，微软拼音）
- `ja-JP` — 日语（Microsoft IME）
- `zh-TW` — 中文（繁体）
- `ko-KR` — 韩语

## 编译

依赖 .NET 9.0 SDK。

```bash
dotnet publish -c Release --no-self-contained -o publish
```

产物为 `publish/ime-switcher.exe`，单文件。若需脱离运行时使用，移除 `--no-self-contained`。

## 协议

MIT
