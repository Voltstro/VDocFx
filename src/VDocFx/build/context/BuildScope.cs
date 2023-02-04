// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Docs.Build;

internal class BuildScope
{
    private readonly Config _config;
    private readonly BuildOptions _buildOptions;
    private readonly Glob _glob;
    private readonly Input _input;
    private readonly HashSet<string> _configReferences;

    // On a case insensitive system, cannot simply get the actual file casing:
    // https://github.com/dotnet/corefx/issues/1086
    // This lookup table stores a list of actual filenames.
    private readonly Watch<(HashSet<FilePath> allFiles, IReadOnlyDictionary<FilePath, ContentType> files)> _files;

    private readonly ConcurrentDictionary<PathString, PathString> _fileMappings = new();

    /// <summary>
    /// Gets all the files and fallback files to build, excluding redirections.
    /// </summary>
    public IEnumerable<FilePath> Files => _files.Value.files.Keys;

    public BuildScope(Config config, Input input, BuildOptions buildOptions)
    {
        _config = config;
        _buildOptions = buildOptions;
        _input = input;
        _glob = new Glob(config.Files, config.Exclude);
        _configReferences = config.Extend.Concat(config.GetFileReferences()).Select(path => PathUtility.Normalize(path.Value))
            .ToHashSet(PathUtility.PathComparer);

        _files = new(GlobFiles);
    }

    public IEnumerable<FilePath> GetFiles(ContentType contentType)
    {
        return from pair in _files.Value.files where pair.Value == contentType select pair.Key;
    }

    public bool TryGetActualFilePath(FilePath path, [NotNullWhen(true)] out FilePath? actualPath)
    {
        if (_files.Value.allFiles.TryGetValue(path, out actualPath))
        {
            return true;
        }

        if (_input.Exists(path))
        {
            actualPath = path;
            return true;
        }

        return false;
    }

    public ContentType GetContentType(FilePath path)
    {
        return path.Origin == FileOrigin.Redirection ? ContentType.Redirection : GetContentType(path.Path);
    }

    public ContentType GetContentType(string path)
    {
        if (_configReferences.Contains(path))
        {
            return ContentType.Unknown;
        }

        var name = Path.GetFileNameWithoutExtension(path);
        if (name.Equals("docfx", PathUtility.PathComparison))
        {
            return ContentType.Unknown;
        }
        if (name.Equals("redirections", PathUtility.PathComparison))
        {
            return ContentType.Unknown;
        }

        if (!path.EndsWith(".md", PathUtility.PathComparison) &&
            !path.EndsWith(".json", PathUtility.PathComparison) &&
            !path.EndsWith(".yml", PathUtility.PathComparison))
        {
            return ContentType.Resource;
        }

        if (name.Equals("toc", PathUtility.PathComparison))
        {
            return ContentType.Toc;
        }

        return ContentType.Page;
    }

    public PathString MapPath(PathString path)
    {
        return _fileMappings.GetOrAdd(path, _ =>
        {
            return path;
        });
    }

    public bool OutOfScope(FilePath file)
    {
        return file.Origin switch
        {
            // Link to dependent repo
            FileOrigin.Dependency when !_config.Dependencies[file.DependencyName].IncludeInBuild => true,

            // Pages outside build scope, don't build the file, leave href as is
            FileOrigin.Main => !_files.Value.files.ContainsKey(file),
            _ => false,
        };
    }

    private (HashSet<FilePath>, IReadOnlyDictionary<FilePath, ContentType>) GlobFiles()
    {
        using (Progress.Start("Globing files"))
        {
            var allFiles = new HashSet<FilePath>();
            var files = new DictionaryBuilder<FilePath, ContentType>();

            var foundFiles = _glob.GetMatchesInDirectory(_input.GetMainPath()).Select(x => FilePath.Content(new PathString(x)));
            allFiles.UnionWith(foundFiles);
            Parallel.ForEach(allFiles, file =>
            {
                files.TryAdd(file, GetContentType(file));
            });

            Parallel.ForEach(_config.Dependencies, dep =>
            {
                var depFiles = _input.ListFilesRecursive(FileOrigin.Dependency, dep.Key);
                lock (allFiles)
                {
                    allFiles.AddRange(depFiles);
                }

                if (dep.Value.IncludeInBuild)
                {
                    Parallel.ForEach(depFiles, file =>
                    {
                        if (_glob.IsMatch(file.Path))
                        {
                            files.TryAdd(file, GetContentType(file));
                        }
                    });
                }
            });

            return (allFiles, files.AsDictionary());
        }
    }
}
