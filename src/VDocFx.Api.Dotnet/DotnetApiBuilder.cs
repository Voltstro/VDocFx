using ECMA2Yaml;
using Microsoft.Docs.Build;
using Mono.Documentation;
using Newtonsoft.Json.Linq;

namespace VDocFx.Api.Dotnet;

internal class DotnetApiBuilder : IApiBuilder
{
    public void Build(JToken config, ErrorBuilder errors, PathString mainDirectory, PathString outputDirectory)
    {
        var dotnetConfig = config.ToObject<DotnetApiConfig>();
        if (dotnetConfig == null)
        {
            errors.Add(new Error(ErrorLevel.Error, "config-load-error", $"Failed to load dotnet API docs!"));
            return;
        }

        var glob = new Glob(dotnetConfig.Assemblies, null);
        var assemblies = glob.GetMatchesInDirectory(mainDirectory);

        var objPath = Path.Combine(AppData.CacheRoot, Guid.NewGuid().ToString(), "obj");
        var dllDirectory = Path.Combine(objPath, "dll");
        var latestDllDirectory = Path.Combine(dllDirectory, "latest");
        var xmlDirectory = Path.Combine(objPath, "xml");
        var apiOutputDirectory = Path.Combine(outputDirectory, dotnetConfig.Dest);

        //Unlikely lmao
        if (Directory.Exists(objPath))
            Directory.Delete(objPath, true);

        Directory.CreateDirectory(latestDllDirectory);
        Directory.CreateDirectory(xmlDirectory);

        using (Log.BeginScope(false))
        {
            Parallel.ForEach(assemblies, assembly =>
            {
                var src = Path.Combine(mainDirectory, Path.GetFullPath(assembly));

                if (!File.Exists(src))
                {
                    errors.Add(new Error(ErrorLevel.Error, "api-file-error", $"Failed to find assembly at {src}!"));
                    return;
                }

                var srcTarget = Path.Combine(latestDllDirectory, Path.GetFileName(assembly));

                Log.Write($"Copy {src} --> {srcTarget}");
                File.Copy(src, srcTarget, overwrite: true);

                var xml = Path.ChangeExtension(src, ".xml");
                if (File.Exists(xml))
                {
                    var xmlTarget = Path.Combine(latestDllDirectory, Path.GetFileName(xml));

                    Log.Write($"Copy {xml} --> {xmlTarget}");
                    File.Copy(xml, xmlTarget, overwrite: true);
                }
            });
        }

        new MDocFrameworksBootstrapper().Run(new[] { "fx-bootstrap", dllDirectory });

        new MDocUpdater().Run(new[]
        {
            "update",
            "-o", xmlDirectory,
            "-fx", dllDirectory,
            "-lang", "docid",
            "-index", "false",
            "--debug", "--delete",
        });

        ECMA2YamlConverter.Run(xmlDirectory, outputDirectory: apiOutputDirectory, config: new() { NoMonikers = true });

        Directory.Delete(objPath, true);
    }
}
