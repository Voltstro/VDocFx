// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal static class New
{
    private static readonly string s_templatePath = Path.Combine(AppContext.BaseDirectory, "data", "new");

    public static bool Run(string? outPut, bool force, bool gitInit, string? template)
    {
        if (string.IsNullOrEmpty(template) ||
            !template.All(ch => char.IsLetterOrDigit(ch) || ch == '-') ||
            !Directory.Exists(Path.Combine(s_templatePath, template)))
        {
            ShowTemplates();
            return true;
        }

        if (force)
        {
            return CreateFromTemplate(template, outPut, gitInit, force, dryRun: false);
        }

        if (CreateFromTemplate(template, outPut, gitInit, force, dryRun: true))
        {
            return true;
        }

        return CreateFromTemplate(template, outPut, gitInit, force, dryRun: false);
    }

    private static void ShowTemplates()
    {
        var width = 20;

        Console.WriteLine("usage: docfx new [<template>]");
        Console.WriteLine();
        Console.WriteLine("Template".PadRight(width, ' ') + "Description");

        try
        {
            Console.WriteLine("".PadRight(Console.BufferWidth - 4, '-'));
        }
        catch
        {
            // Console.BufferWidth sometimes throw
        }

        foreach (var template in Directory.GetDirectories(s_templatePath))
        {
            var name = Path.GetFileName(template);
            var description = File.ReadAllText(Path.Combine(template, "__description")).Trim();

            Console.WriteLine(name.PadRight(width, ' ') + description);
            Console.WriteLine();
        }
    }

    private static bool CreateFromTemplate(string templateName, string? output, bool gitInit, bool force, bool dryRun)
    {
        output ??= ".";
        var overwriteFiles = new List<string>();

        foreach (var file in Directory.GetFiles(Path.Combine(s_templatePath, templateName)))
        {
            if (Path.GetFileName(file).StartsWith("__"))
            {
                continue;
            }

            var target = Path.GetRelativePath(Path.Combine(s_templatePath, templateName), file);
            var targetFullPath = Path.GetFullPath(Path.Combine(output, target));

            if (dryRun)
            {
                if (File.Exists(targetFullPath))
                {
                    overwriteFiles.Add(target);
                }
            }
            else
            {
                var directory = Path.GetDirectoryName(targetFullPath) ?? ".";
                Directory.CreateDirectory(directory);
                if (gitInit)
                {
                    GitUtility.Init(directory);
                }
                File.Copy(file, targetFullPath, overwrite: force);
            }
        }

        if (dryRun)
        {
            if (overwriteFiles.Count <= 0)
            {
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Creating this template will make changes to existing files:");

            foreach (var overwriteFile in overwriteFiles)
            {
                Console.WriteLine("  " + overwriteFile);
            }

            Console.WriteLine();
            Console.WriteLine("Rerun the command and pass --force to accept and create.");
            Console.ResetColor();
            return true;
        }

        Console.WriteLine($"The template \"{templateName}\" was created successfully.");

        return true;
    }
}
