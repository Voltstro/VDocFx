// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO.Pipelines;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;

namespace Microsoft.Docs.Build;

internal static class Serve
{
    public static bool Run(string address, int port)
    {
        var url = $"http://{address}:{port}/";
        var builder = WebApplication.CreateBuilder();

        builder.WebHost
            .ConfigureLogging(options => options.ClearProviders())
            .UseUrls(url);

        using var app = builder.Build();

        if (!ServeStaticFiles(app))
        {
            return true;
        }

        app.Start();

        LogServerAddress(app);
        app.WaitForShutdown();

        return false;

        void LogServerAddress(IApplicationBuilder app)
        {
            var urls = app.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
            if (urls != null)
            {
                foreach (var url in urls)
                {
                    Console.WriteLine($"  {url}");
                }
            }
            Console.WriteLine("Press Ctrl+C to shut down.");
        }
    }

    private static bool ServeStaticFiles(IApplicationBuilder app)
    {
        var publishFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), ".publish.json", SearchOption.AllDirectories);
        if (!publishFiles.Any())
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"No files to serve, did you forget to run 'docfx build'?");
            Console.ResetColor();
            return false;
        }

        foreach (var publishFile in publishFiles)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(publishFile));
            PrintServeDirectory(directory);
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(directory),
            });
        }

        return true;
    }

    private static void PrintServeDirectory(string? directory)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("Serving ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(directory);
        Console.ResetColor();
    }
}
