// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class Builder
{
    private readonly ScopedErrorBuilder _errors = new();
    private readonly ScopedProgressReporter _progressReporter = new();
    private readonly Watch<DocsetBuilder[]> _docsets;
    private readonly Package _package;
    private readonly CredentialProvider? _getCredential;

    private readonly string _output;
    private readonly bool _dryRun;
    private readonly bool _noRestore;
    private readonly bool _noCache;
    private readonly OutputType? _outputType;

    public Builder(Package package, string output, bool dryRun, bool noRestore, bool noCache, OutputType? outputType,
        CredentialProvider? getCredential = null)
    {
        _package = package;
        _output = output;
        _dryRun = dryRun;
        _noRestore = noRestore;
        _noCache = noCache;
        _outputType = outputType;
        _getCredential = getCredential;
        _docsets = new(LoadDocsets);
    }

    public static bool Run(string output, bool dryRun, bool noRestore, bool noCache, OutputType? outputType, Package? package = null)
    {
        using (Watcher.Disable())
        {
            using var errors = new ErrorWriter();

            package ??= new LocalPackage();

            new Builder(package, output, dryRun, noRestore, noCache, outputType).Build(errors, new ConsoleProgressReporter());

            errors.PrintSummary();
            return errors.HasError;
        }
    }

    public void Build(ErrorBuilder errors, IProgress<string> progressReporter, string[]? files = null)
    {
        if (files?.Length == 0)
        {
            return;
        }

        using (Watcher.BeginScope())
        using (_errors.BeginScope(errors))
        using (_progressReporter.BeginScope(progressReporter))
        {
            try
            {
                Parallel.ForEach(
                    _docsets.Value,
                    docset => docset.Build(files is null ? null : Array.ConvertAll(files, path => GetPathToDocset(docset, path))));
            }
            catch (Exception ex) when (DocfxException.IsDocfxException(ex, out var dex))
            {
                _errors.AddRange(dex);
            }
        }
    }

    private DocsetBuilder[] LoadDocsets()
    {
        _progressReporter.Report("Loading docsets...");

        // load and trace entry repository
        var repository = Repository.Create(_package.BasePath);

        var docsets = ConfigLoader.FindDocsets(_errors, _package, _output, repository);
        if (docsets.Length == 0)
        {
            _errors.Add(Errors.Config.ConfigNotFound(Directory.GetCurrentDirectory()));
        }

        return (from docset in docsets
                let item = DocsetBuilder.Create(
                    _errors,
                    repository,
                    docset.docsetPath,
                    _noRestore,
                    _noCache,
                    docset.outputPath,
                    _outputType,
                    _package.CreateSubPackage(docset.docsetPath),
                    _progressReporter,
                    _getCredential)
                where item != null
                select item).ToArray();
    }

    private string GetPathToDocset(DocsetBuilder docset, string file)
    {
        return Path.GetRelativePath(docset.BuildOptions.DocsetPath, Path.Combine(".", file));
    }
}
