# Ask AI —— PowerToys 命令面板「问 AI」扩展

在 **PowerToys Command Palette（命令面板）** 里输入一句话，回车选择目标，即可：

- 直接**打开浏览器（Tabbit）**并把这句话作为搜索/对话发给指定 AI 网站；
- 或**打开终端运行 Claude Code**，把这句话作为 prompt。

> 结论先行：截至 2026-10，社区已有「LLM Extension for Command Palette」「Command Palette LLM」「Claude for Command Palette」等扩展，但它们要么**在面板内聊天**、要么**打开 Claude 桌面应用**，**没有**「输入一句话 → 打开浏览器里的 AI / 调起 Claude Code」的成品。所以这里给你**手写了一个可用扩展**。

---

## 一、效果

1. 打开命令面板（默认 `Win + Alt + Space`），输入 `Ask AI` 回车进入本扩展。
2. 在搜索框里输入要对 AI 说的话，例如：`帮我总结一下今天的新闻`。
3. 列表里出现你的目标，例如：
   - `在浏览器里问 AI（Tabbit）` → 回车：用默认浏览器（Tabbit）打开对应 AI 网站，并把这句话填进去。
   - `在 Claude Code 里执行` → 回车：弹出终端窗口运行 `claude "帮我总结一下今天的新闻"`。
4. 面板自动关闭，焦点交给刚打开的应用。

---

## 二、环境要求

- Windows 11（或 Windows 10 19041+）
- 已安装 **PowerToys** 且启用 **Command Palette**（命令面板）
- **Visual Studio 2022**（勾选「使用 C++ 的桌面开发」「.NET 桌面开发」「Windows 应用 SDK / WinUI」工作负载）
- 开启 Windows **开发者模式**
- .NET 9 SDK

---

## 三、编译与安装

### 方式 A：Visual Studio（推荐）

1. 双击打开 `AskAiExtension.sln`。
2. 顶部工具栏**平台选 `x64`**（或 ARM64），配置选 `Debug`。
3. 菜单 **生成 → 部署 AskAiExtension**（注意是「部署」，不是只「生成」；只生成不会把 MSIX 注册进系统）。
4. 回到命令面板，输入 `Reload`，选择副标题为 `Reload Command Palette Extension` 的那一项回车。
5. 再输入 `Ask AI`，即可看到扩展。

### 方式 B：命令行

```powershell
# 还原 + 生成
dotnet build .\AskAiExtension\AskAiExtension.csproj -c Debug -p:Platform=x64

# 打包 MSIX（可选）
dotnet publish .\AskAiExtension\AskAiExtension.csproj `
  -c Release -p:Platform=x64 -p:RuntimeIdentifier=win-x64
```

> 命令行方式只生成程序集；要让它出现在命令面板里，仍需用 VS 的「部署」，或手动注册生成的 MSIX 包。

---

## 四、配置（重要）

首次运行会自动生成配置文件：

```
%LOCALAPPDATA%\AskAiExtension\config.json
```

可直接编辑它来增删目标（改完在面板里 `Reload` 一次即可）。结构如下：

```json
{
  "Targets": [
    {
      "Name": "在浏览器里问 AI（Tabbit）",
      "Kind": "browser",
      "UrlTemplate": "https://www.perplexity.ai/search?q={prompt}",
      "BrowserPath": ""
    },
    {
      "Name": "在 Claude Code 里执行",
      "Kind": "terminal",
      "CommandTemplate": "claude \"{prompt}\"",
      "WorkingDirectory": ""
    }
  ]
}
```

字段说明：

| 字段 | 说明 |
| --- | --- |
| `Name` | 面板里显示的名字 |
| `Kind` | `browser`（打开网址）或 `terminal`（跑命令） |
| `UrlTemplate` | `browser` 用。`{prompt}` 会替换成 **URL 编码**后的问题 |
| `CommandTemplate` | `terminal` 用。`{prompt}` 会替换成**原样**的问题 |
| `BrowserPath` | 留空 = 用系统默认浏览器（即你的 Tabbit）；也可填绝对路径强制指定 |
| `WorkingDirectory` | `terminal` 用，运行命令前 `cd` 到的目录（留空 = 用户主目录） |

### 常用 AI 的 UrlTemplate 参考

| AI | UrlTemplate |
| --- | --- |
| Perplexity | `https://www.perplexity.ai/search?q={prompt}` |
| 豆包 | `https://www.doubao.com/chat/?q={prompt}` |
| Kimi | `https://kimi.moonshot.cn/?q={prompt}` |
| ChatGPT | `https://chatgpt.com/?q={prompt}` |

> 各站点对「用 URL 预填问题」的支持会变化；若某站不识别参数，会退化成只打开首页，此时可改用该站的搜索页地址。

### 「跳转到 Tabbit 浏览器里的 AI」

系统默认浏览器设为 **Tabbit**，且 `BrowserPath` 留空即可 —— 扩展会用 `Process.Start(url, UseShellExecute=true)` 交给默认浏览器打开，从而落到 Tabbit 里的 AI 页面。

---

## 五、目录结构

```
AskAiExtension-CommandPalette/
├─ .gitignore
├─ AskAiExtension.sln
├─ Directory.Build.props          # 平台/分析器
├─ Directory.Packages.props       # 集中包版本（含 CommandPalette.Extensions）
├─ nuget.config
└─ AskAiExtension/
   ├─ AskAiExtension.csproj
   ├─ app.manifest
   ├─ Package.appxmanifest        # MSIX + COM server + CmdPal 扩展声明
   ├─ Program.cs                  # COM 服务入口
   ├─ AskAiExtension.cs           # IExtension 实现
   ├─ AskAiExtensionCommandsProvider.cs
   ├─ Config.cs                   # 配置模型与读写
   ├─ Launcher.cs                 # 打开浏览器 / 运行终端
   ├─ Pages/AskAiExtensionPage.cs # 主页面 + 启动命令
   ├─ Properties/...              # launchSettings / 发布配置
   └─ Assets/                     # 图标
```

---

## 六、已知限制

- 扩展基于 **预览版** 的 Command Palette 扩展 API，PowerToys 升级后 API 可能变化，需要重新适配。
- MSIX 的 `Publisher` / `Identity` 目前是示例值（`CN=AskAiExtension`）。正式分发时请改成你自己的证书主题，否则别人装不上。
- 「用 URL 预填 prompt」是否生效取决于各 AI 站点，扩展只负责拼 URL 并打开。
- 本扩展**只在本地运行**，不收集、不上传任何数据；prompt 由你配置的目标站点/命令行直接处理。
