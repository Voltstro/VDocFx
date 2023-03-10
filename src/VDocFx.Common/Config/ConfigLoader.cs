// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Microsoft.Docs.Build;

internal static class ConfigLoader
{
    public static (string docsetPath, string? outputPath)[] FindDocsets(
        ErrorBuilder errors, Package package, string? output, Repository? repository)
    {
        var glob = FindDocsetsGlob(errors, package, repository);
        if (glob is null)
        {
            return new[] { (package.BasePath.Value, output) };
        }

        var files = package.GetFiles(allowedFileNames: new string[] { "vdocfx.json", "vdocfx.yml" });

        return (
            from file in files
            where glob.IsMatch(file)
            let fullPath = package.GetFullFilePath(file)
            let docsetPath = Path.GetDirectoryName(fullPath)
            let docsetFolder = Path.GetDirectoryName(file)
            let outputPath = string.IsNullOrEmpty(output) ? null : Path.Combine(output, docsetFolder)
            select (docsetPath, outputPath)).Distinct().ToArray();
    }

    /// <summary>
    /// Load the config under <paramref name="docsetPath"/>
    /// </summary>
    public static (Config, BuildOptions, PackageResolver, FileResolver) Load(
        ErrorBuilder errors,
        Repository? repository,
        string docsetPath,
        string? outputPath,
        FetchOptions fetchOptions,
        Package package,
        CredentialProvider? getCredential = null)
    {
        var docfxConfig = LoadConfig(errors, package);
        if (docfxConfig is null)
        {
            throw Errors.Config.ConfigNotFound(package.BasePath).ToException();
        }

        var unionProperties = new string[] { "xref" };

        // Load configs available locally
        var envConfig = LoadEnvironmentVariables(Environment.GetEnvironmentVariables().Cast<DictionaryEntry>());
        var cliConfig = new JObject();

        var globalConfig = LoadConfig(errors, package, new PathString(AppData.Root));

        // Preload
        var preloadConfigObject = new JObject();
        JsonUtility.Merge(unionProperties, preloadConfigObject, envConfig, globalConfig, docfxConfig, cliConfig);
        var preloadConfig = JsonUtility.ToObject<PreloadConfig>(errors, preloadConfigObject);

        // Download dependencies
        var credentialProviders = new List<CredentialProvider>();
        if (getCredential != null)
        {
            credentialProviders.Add(getCredential);
        }
        credentialProviders.Add((url, _, _) => Task.FromResult(preloadConfig.Secrets.GetHttpConfig(url)));

        PackageResolver? packageResolver = default;
        var fallbackDocsetPath = new Lazy<string?>(
            () => LocalizationUtility.GetFallbackDocsetPath(docsetPath, repository, preloadConfig.FallbackRepository, packageResolver!));
        var fileResolver = new FileResolver(package, fallbackDocsetPath, fetchOptions);

        packageResolver = new PackageResolver(errors, docsetPath, preloadConfig, fetchOptions, fileResolver, repository);

        var buildOptions = new BuildOptions(docsetPath, fallbackDocsetPath.Value, outputPath, repository, preloadConfig, package);
        var extendConfig = DownloadExtendConfig(
            errors, buildOptions.Locale, preloadConfig, null, null, repository, preloadConfig.PublishRepositoryUrl, fileResolver);

        // Create full config
        var configObject = new JObject();
        JsonUtility.Merge(unionProperties, configObject, envConfig, globalConfig, extendConfig, docfxConfig, cliConfig);
        var config = JsonUtility.ToObject<Config>(errors, configObject);
        return (config, buildOptions, packageResolver, fileResolver);
    }

    internal static JObject LoadEnvironmentVariables(IEnumerable<DictionaryEntry> environmentVariables)
    {
        var root = new JObject();

        foreach (var entry in environmentVariables)
        {
            var key = entry.Key.ToString();
            if (key is null || !key.StartsWith("DOCFX_"))
            {
                continue;
            }

            var value = entry.Value?.ToString();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            var segments = key["DOCFX".Length..].Split("__", StringSplitOptions.RemoveEmptyEntries);
            var container = ExpandProperties(root, segments);
            if (container is null)
            {
                continue;
            }

            var name = StringUtility.ToCamelCase('_', segments[^1]);
            container[name] = GetJsonValue(value);
        }

        return root;

        static JObject? ExpandProperties(JObject root, string[] segments)
        {
            var current = root;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var name = StringUtility.ToCamelCase('_', segments[i]);
                if (!current.TryGetValue(name, out var item))
                {
                    current = (JObject)(current[name] = new JObject());
                }
                else if (item is JObject obj)
                {
                    current = obj;
                }
                else
                {
                    return null;
                }
            }
            return current;
        }

        static JToken GetJsonValue(string value)
        {
            var trimmedValue = value.Trim();
            if (trimmedValue.StartsWith('{') && trimmedValue.EndsWith('}'))
            {
                try
                {
                    return JObject.Parse(value);
                }
                catch
                {
                }
            }

            var values = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return values.Length == 1 ? values[0] : new JArray(values);
        }
    }

    private static JObject? LoadConfig(ErrorBuilder errors, Package package, PathString directory = default)
    {
        var config = package.TryLoadYamlOrJson<JObject>(errors, "vdocfx", directory);
        if (config is null)
        {
            return null;
        }

        // For v2 backward compatibility, treat `build` section as config if it exist
        if (config.TryGetValue("build", out var build) && build is JObject buildObj)
        {
            // `template` property has different semantic, so remove it
            buildObj.Remove("template");
            return buildObj;
        }

        return config;
    }

    private static JObject DownloadExtendConfig(
        ErrorBuilder errors,
        string? locale,
        PreloadConfig config,
        string? xrefEndpoint,
        string[]? xrefQueryTags,
        Repository? repository,
        string? publishRepositoryUrl,
        FileResolver fileResolver)
    {
        var result = new JObject();
        var extendQuery =
            $"name={WebUtility.UrlEncode(config.Name)}" +
            $"&locale={WebUtility.UrlEncode(locale)}" +
            $"&repository_url={WebUtility.UrlEncode(repository?.Url)}" +
            $"&publish_repository_url={WebUtility.UrlEncode(publishRepositoryUrl)}" +
            $"&branch={WebUtility.UrlEncode(repository?.Branch ?? "main")}" +
            $"&xref_endpoint={WebUtility.UrlEncode(xrefEndpoint)}" +
            $"&xref_query_tags={WebUtility.UrlEncode(xrefQueryTags is null ? null : string.Join(',', xrefQueryTags))}";

        foreach (var extend in config.Extend)
        {
            var extendWithQuery = extend;
            if (UrlUtility.IsHttp(extend))
            {
                extendWithQuery = new SourceInfo<string>(UrlUtility.MergeUrl(extend, extendQuery), extend);
            }

            var extendContent = fileResolver.ReadString(extendWithQuery);
            var extendConfigObject = extend.Value.EndsWith(".yml", PathUtility.PathComparison)
                ? YamlUtility.Deserialize<JObject>(errors, extendContent, new FilePath(extend))
                : JsonUtility.Deserialize<JObject>(errors, extendContent, new FilePath(extend));

            JsonUtility.Merge(result, extendConfigObject);
        }

        return result;
    }

    private static Glob? FindDocsetsGlob(ErrorBuilder errors, Package package, Repository? repository)
    {
        var config = package.TryLoadYamlOrJson<DocsetsConfig>(errors, "docsets");
        if (config != null)
        {
            return new Glob(config.Docsets, config.Exclude);
        }

        return null;
    }
}
