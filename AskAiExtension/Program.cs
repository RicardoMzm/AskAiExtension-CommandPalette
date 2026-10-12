// Ask AI - Command Palette extension entry point.
// This is the COM server host that Command Palette launches.

using System;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace AskAiExtension;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
            {
                using ExtensionServer server = new();
                var extensionDisposedEvent = new ManualResetEvent(false);
                var extensionInstance = new AskAiExtension(extensionDisposedEvent);

                // A single extension instance is returned every time the host asks for it.
                server.RegisterExtension(() => extensionInstance);

                // Keep the process alive until the extension is disposed by the host.
                extensionDisposedEvent.WaitOne();
            }
            else
            {
                Console.WriteLine("Not being launched as an Extension... exiting.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unhandled exception caught: {e}");
        }
    }
}
