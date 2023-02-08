// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal static class Restore
{
    public static void RestoreDocset(
        ErrorBuilder errors, Config config, PackageResolver packageResolver, FileResolver fileResolver)
    {
        // download dependencies to disk
        Parallel.Invoke(
            () => RestoreFiles(errors, config, fileResolver),
            () => RestorePackages(errors, config, packageResolver));
    }

    private static void RestoreFiles(ErrorBuilder errors, Config config, FileResolver fileResolver)
    {
        using var scope = Progress.Start("Restoring files");
        ParallelUtility.ForEach(scope, errors, config.GetFileReferences(), fileResolver.Download);
    }

    private static void RestorePackages(ErrorBuilder errors, Config config, PackageResolver packageResolver)
    {
        using var scope = Progress.Start("Restoring packages");
        ParallelUtility.ForEach(
            scope,
            errors,
            GetPackages(config).Distinct(),
            item => packageResolver.DownloadPackage(item.package, item.flags));
    }

    private static IEnumerable<(PackagePath package, PackageFetchOptions flags)> GetPackages(Config config)
    {
        foreach (var (_, package) in config.Dependencies)
        {
            yield return (package, package.PackageFetchOptions);
        }

        if (config.Template.Type == PackageType.Git)
        {
            yield return (config.Template, PackageFetchOptions.DepthOne | PackageFetchOptions.IgnoreBranchFallbackError);
        }
    }
}
