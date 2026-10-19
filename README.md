# Ask AI · PowerToys 命令面板扩展

在命令面板里打一句话，回车就能打开浏览器里的 AI，或者直接调起 Claude Code 把这句话跑起来。

## 怎么用

1. `Win + Alt + Space` 打开命令面板，输入 `Ask AI` 回车。
2. 输入你想问的话，比如「帮我总结一下今天的新闻」。
3. 选一个目标：
   - `在浏览器里问 AI（Tabbit）`：打开默认浏览器，把问题带过去
   - `在 Claude Code 里执行`：弹一个终端，运行 `claude "..."`

选完面板会自动关闭，焦点交给新打开的应用。

## 装之前

需要这些：

- Windows 11（或 Win10 19041+）
- PowerToys 的命令面板已经打开
- Visual Studio 2022，装了「C++ 桌面开发」「.NET 桌面开发」「Windows 应用 SDK / WinUI」
- .NET 9 SDK
- Windows 开发者模式

## 编译 & 安装

用 VS 打开 `AskAiExtension.sln`，平台选 `x64`，然后 **生成 → 部署 AskAiExtension**。

注意是「部署」，只点「生成」不会把扩展注册到系统。

装完在命令面板里输入 `Reload`，选 `Reload Command Palette Extension`，再搜 `Ask AI` 就能看到。

命令行也能编译：

```powershell
dotnet build .\AskAiExtension\AskAiExtension.csproj -c Debug -p:Platform=x64
```

但要让它出现在面板里，还是得用 VS 部署，或者手动注册 MSIX。

## 配置

第一次运行会生成：

```
%LOCALAPPDATA%\AskAiExtension\config.json
```

想加目标就改这个文件：

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

几个字段：

- `Kind`：`browser` 打开网址，`terminal` 跑命令
- `UrlTemplate` 里的 `{prompt}` 会做 URL 编码；`CommandTemplate` 里是原样替换
- `BrowserPath` 留空就用默认浏览器（你的 Tabbit）
- `WorkingDirectory` 留空就是用户主目录

改完在面板里 `Reload` 一下。

常用站点的 URL：

| AI | UrlTemplate |
| --- | --- |
| Perplexity | `https://www.perplexity.ai/search?q={prompt}` |
| 豆包 | `https://www.doubao.com/chat/?q={prompt}` |
| Kimi | `https://kimi.moonshot.cn/?q={prompt}` |
| ChatGPT | `https://chatgpt.com/?q={prompt}` |

有些站点不认 `?q=`，那就只会打开首页，换个搜索页地址试试。

## 小提醒

- 这是基于预览版 Command Palette API 的，PowerToys 大版本更新后可能要重新适配
- MSIX 的 `Publisher` / `Identity` 还是示例值，真要发给别人得换成你自己的证书
- 所有东西都在本地跑，prompt 只发给你自己配置的网站或命令行