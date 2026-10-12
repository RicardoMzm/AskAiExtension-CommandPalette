// Ask AI - user configuration model and persistence.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AskAiExtension;

/// <summary>
/// One destination that a typed prompt can be sent to.
/// Kind == "browser"  : open <see cref="UrlTemplate"/> in the browser ({prompt} is URL-encoded).
/// Kind == "terminal" : run <see cref="CommandTemplate"/> in a terminal ({prompt} is inserted raw).
/// </summary>
internal sealed class AiTarget
{
    public string Name { get; set; } = "AI";

    public string Kind { get; set; } = "browser";

    /// <summary>URL with an optional {prompt} placeholder. Used when Kind == "browser".</summary>
    public string UrlTemplate { get; set; } = string.Empty;

    /// <summary>Shell command with an optional {prompt} placeholder. Used when Kind == "terminal".</summary>
    public string CommandTemplate { get; set; } = string.Empty;

    /// <summary>Optional explicit browser executable. Empty = system default browser (Tabbit).</summary>
    public string BrowserPath { get; set; } = string.Empty;

    /// <summary>Optional working directory for terminal targets.</summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}

internal sealed class Config
{
    public List<AiTarget> Targets { get; set; } = DefaultTargets();

    private static List<AiTarget> DefaultTargets() =>
    [
        new AiTarget
        {
            Name = "在浏览器里问 AI（Tabbit）",
            Kind = "browser",
            UrlTemplate = "https://www.perplexity.ai/search?q={prompt}",
            BrowserPath = string.Empty,
        },
        new AiTarget
        {
            Name = "在 Claude Code 里执行",
            Kind = "terminal",
            CommandTemplate = "claude \"{prompt}\"",
            WorkingDirectory = string.Empty,
        },
    ];
}

internal static class ConfigStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AskAiExtension",
        "config.json");

    public static Config Load()
    {
        try
        {
            var path = ConfigPath;
            var dir = Path.GetDirectoryName(path)!;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(path))
            {
                var fresh = new Config();
                File.WriteAllText(path, JsonSerializer.Serialize(fresh, Options));
                return fresh;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json, Options) ?? new Config();
        }
        catch
        {
            return new Config();
        }
    }
}
