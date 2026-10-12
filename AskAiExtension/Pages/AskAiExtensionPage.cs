// Ask AI - the Command Palette page and the command that launches a target.

using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AskAiExtension.Pages;

/// <summary>
/// The main page: type a prompt in the search box, each row is a destination
/// (e.g. "open Tabbit browser AI" or "run Claude Code"). Select a row to launch.
/// </summary>
internal sealed partial class AskAiExtensionPage : DynamicListPage
{
    private readonly Config _config;

    public AskAiExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\AskAiExtensionLogo.png");
        Title = "Ask AI";
        Name = "Ask AI (Tabbit / Claude Code)";
        PlaceholderText = "输入要对 AI 说的话，然后回车选择目标…";
        _config = ConfigStore.Load();
    }

    // The page is dynamic: whenever the search text changes, re-render the rows
    // so the subtitle previews the prompt that will be sent.
    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        try
        {
            var prompt = SearchText ?? string.Empty;
            var items = new List<IListItem>();

            foreach (var target in _config.Targets)
            {
                var isTerminal = string.Equals(target.Kind, "terminal", StringComparison.OrdinalIgnoreCase);
                items.Add(new ListItem(new OpenAiTargetCommand(target, () => SearchText))
                {
                    Title = target.Name,
                    Subtitle = string.IsNullOrWhiteSpace(prompt)
                        ? (isTerminal ? "在终端中运行（当前没有输入）" : "打开浏览器（当前没有输入）")
                        : $"发送：{Truncate(prompt, 90)}",
                    Icon = new IconInfo(isTerminal ? "\uE756" : "\uE774"),
                });
            }

            if (items.Count == 0)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "还没有配置目标",
                    Subtitle = $"请编辑 {ConfigStore.ConfigPath}",
                });
            }

            return items.ToArray();
        }
        catch (Exception ex)
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "出错了",
                    Subtitle = ex.Message,
                    Icon = new IconInfo("\u26A0"),
                },
            ];
        }
    }

    private static string Truncate(string s, int max)
    {
        s = s.Replace("\r", " ").Replace("\n", " ");
        return s.Length <= max ? s : s[..max] + "…";
    }
}

/// <summary>Invokable command that launches one configured AI target with the current prompt.</summary>
internal sealed partial class OpenAiTargetCommand : InvokableCommand
{
    private readonly AiTarget _target;
    private readonly Func<string> _prompt;

    public OpenAiTargetCommand(AiTarget target, Func<string> prompt)
    {
        _target = target;
        _prompt = prompt;
        Name = target.Name;
        Icon = new IconInfo(string.Equals(target.Kind, "terminal", StringComparison.OrdinalIgnoreCase) ? "\uE756" : "\uE774");
    }

    public override ICommandResult Invoke()
    {
        var prompt = (_prompt() ?? string.Empty).Trim();

        try
        {
            if (string.Equals(_target.Kind, "terminal", StringComparison.OrdinalIgnoreCase))
            {
                Launcher.RunInTerminal(
                    Launcher.BuildCommand(_target.CommandTemplate, prompt),
                    _target.WorkingDirectory);
            }
            else
            {
                Launcher.OpenUrl(
                    Launcher.BuildUrl(_target.UrlTemplate, prompt),
                    _target.BrowserPath);
            }

            // Hand the desktop over to the app we just launched and reset the palette.
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs
            {
                Message = "启动失败：" + ex.Message,
            });
        }
    }
}
