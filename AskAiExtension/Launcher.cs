// Ask AI - process launching helpers.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AskAiExtension;

internal static class Launcher
{
    /// <summary>Substitute {prompt} into a URL template, URL-encoding the prompt.</summary>
    public static string BuildUrl(string template, string prompt)
    {
        var encoded = Uri.EscapeDataString(prompt);
        return template.Contains("{prompt}", StringComparison.Ordinal)
            ? template.Replace("{prompt}", encoded, StringComparison.Ordinal)
            : template;
    }

    /// <summary>Substitute {prompt} into a shell command template (raw, not encoded).</summary>
    public static string BuildCommand(string template, string prompt)
    {
        return template.Contains("{prompt}", StringComparison.Ordinal)
            ? template.Replace("{prompt}", prompt, StringComparison.Ordinal)
            : template;
    }

    /// <summary>Open a URL. If browserPath is set it is used explicitly, otherwise the system default browser.</summary>
    public static void OpenUrl(string url, string browserPath)
    {
        if (!string.IsNullOrWhiteSpace(browserPath) && File.Exists(browserPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = browserPath,
                Arguments = "\"" + url + "\"",
                UseShellExecute = true,
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
    }

    /// <summary>Run a command in a terminal window. Uses a temp .cmd file to avoid quoting problems.</summary>
    public static void RunInTerminal(string command, string workingDirectory)
    {
        var script = Path.Combine(Path.GetTempPath(), $"askai_{Guid.NewGuid():N}.cmd");
        var lines = new List<string> { "@echo off" };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            lines.Add($"cd /d \"{workingDirectory}\"");
        }

        lines.Add(command);
        lines.Add("echo.");
        lines.Add("pause");

        File.WriteAllText(script, string.Join("\r\n", lines));

        try
        {
            // Prefer Windows Terminal if present.
            Process.Start(new ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = $"cmd /k \"{script}\"",
                UseShellExecute = true,
            });
        }
        catch
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k \"{script}\"",
                UseShellExecute = true,
            });
        }
    }
}
