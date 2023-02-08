// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Microsoft.Docs.Build;

public static class Docfx
{
    internal static int Main(params string[] args)
    {
        try
        {
            return Run(args);
        }
        catch
        {
            return -99999;
        }
    }

    internal static int Run(string[] args, Package? package = null)
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        var minThreads = Math.Max(32, Environment.ProcessorCount * 4);
        ThreadPool.SetMinThreads(minThreads, minThreads);

        var rootCommand = new RootCommand()
        {
            NewCommand(),
            BuildCommand(package),
            ServeCommand(),
        };

        try
        {
            return rootCommand.Invoke(args);
        }
        catch (Exception ex)
        {
            PrintFatalErrorMessage(ex);
            throw;
        }
    }

    private static Command NewCommand()
    {
        var outputOption = new Option<string>(
            new[] { "-o", "--output" }, "Output directory in which to place built artifacts.");
        var forceOption = new Option<bool>(
            "--force", "Forces content to be generated even if it would change existing files.");
        var gitInitOption = new Option<bool>("--git-init", "Initialize the docset as a git directory.");
        var templateOption = new Argument<string>("templateName", "Docset template name") { Arity = ArgumentArity.ZeroOrOne };

        var command = new Command("new", "Creates a new docset.");
        command.AddOption(outputOption);
        command.AddOption(forceOption);
        command.AddOption(gitInitOption);
        command.AddArgument(templateOption);

        command.SetHandler((output, force, gitInit, template) =>
        {
            New.Run(output, force, gitInit, template);
        }, outputOption, forceOption, gitInitOption, templateOption);

        return command;
    }

    private static Command BuildCommand(Package? package)
    {
        var outputOption = new Option<string>(
            new[] { "-o", "--output" }, "Output directory in which to place built artifacts.");
        var dryRunOption = new Option<bool>(
            "--dry-run", "Do not produce build artifact and only produce validation result.");
        var noRestoreOption = new Option<bool>(
            "--no-restore", "Do not restore dependencies before build.");
        var noCacheOption = new Option<bool>(
            "--no-cache", "Always fetch latest dependencies in build.");

        var command = new Command("build", "Builds a docset.");
        command.AddOption(outputOption);
        command.AddOption(dryRunOption);
        command.AddOption(noRestoreOption);
        command.AddOption(noCacheOption);

        command.SetHandler((output, dryRun, noRestore, noCache) =>
        {
            Builder.Run(output, dryRun, noRestore, noCache, package);
        }, outputOption, dryRunOption, noRestoreOption, noCacheOption);

        return command;
    }

    private static Command ServeCommand()
    {
        var addressOption = new Option<string>(
            "--address", () => "127.0.0.1", "Address to use.");
        var portOption = new Option<int>(
            "--port", () => 8080, "Port to use. If 0, look for open port.");

        var command = new Command("serve", "Serves content in a docset.");
        command.AddOption(addressOption);
        command.Add(portOption);

        command.SetHandler((address, port) =>
        {
            Serve.Run(address, port);
        }, addressOption, portOption);

        return command;
    }

    private static void PrintFatalErrorMessage(Exception exception)
    {
        while (exception is AggregateException ae && ae.InnerException != null)
        {
            exception = ae.InnerException;
        }

        Console.ResetColor();
        Console.WriteLine();

        // windows command line does not have good emoji support
        // https://github.com/Microsoft/console/issues/190
        var showEmoji = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (showEmoji)
        {
            Console.Write("🚘💥🚗 ");
        }
        Console.Write("docfx has crashed");
        if (showEmoji)
        {
            Console.Write(" 🚘💥🚗");
        }
        Console.WriteLine();

        var body = $@"
# docfx crash report: {exception.GetType()}

docfx: `{GetDocfxVersion()}`
dotnet: `{GetDotnetVersion()}`
x64: `{Environment.Is64BitProcess}`
os: `{RuntimeInformation.OSDescription}`
git: `{GetGitVersion()}`
{GetDocfxEnvironmentVariables()}
## repro steps

Run `{Environment.CommandLine}` in `{Directory.GetCurrentDirectory()}`

## callstack

```
{exception}
```
";
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(body);
        Console.ResetColor();
    }

    private static string GetDocfxEnvironmentVariables()
    {
        try
        {
            return string.Concat(
                from entry in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                let key = entry.Key.ToString()
                where key != null && key.StartsWith("DOCFX_")
                select $"{entry.Key}: `{entry.Value}`\n");
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static string? GetDocfxVersion()
    {
        return GetVersion(typeof(Docfx));
    }

    private static string? GetVersion(Type type)
    {
        try
        {
            return type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static string? GetDotnetVersion()
    {
        try
        {
            var process = System.Diagnostics.Process.Start(
                new ProcessStartInfo { FileName = "dotnet", Arguments = "--version", RedirectStandardOutput = true });
            process?.WaitForExit(2000);
            return process?.StandardOutput.ReadToEnd().Trim();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static string? GetGitVersion()
    {
        try
        {
            var process = System.Diagnostics.Process.Start(
                new ProcessStartInfo { FileName = "git", Arguments = "--version", RedirectStandardOutput = true });
            process?.WaitForExit(2000);
            return process?.StandardOutput.ReadToEnd().Trim();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
