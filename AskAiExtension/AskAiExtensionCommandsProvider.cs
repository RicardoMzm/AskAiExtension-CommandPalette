// Ask AI - Command Palette extension.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AskAiExtension;

public partial class AskAiExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public AskAiExtensionCommandsProvider()
    {
        DisplayName = "Ask AI";
        Icon = IconHelpers.FromRelativePath("Assets\\AskAiExtensionLogo.png");
        _commands =
        [
            new CommandItem(new Pages.AskAiExtensionPage())
            {
                Title = "Ask AI (Tabbit / Claude Code)",
                Subtitle = "输入一句话，直接打开浏览器里的 AI 或 Claude Code 对话",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
