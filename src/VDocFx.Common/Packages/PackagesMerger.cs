namespace Microsoft.Docs.Build;

/// <summary>
///     Merges <see cref="Package"/>
/// </summary>
internal sealed class PackagesMerger : Package
{
    private readonly Package[] packages;

    public PackagesMerger(params Package[] packages)
    {
        this.packages = packages;
    }

    public override PathString BasePath => packages[0].BasePath;

    public override bool Exists(PathString path)
    {
        foreach (var package in packages)
        {
            if (package.Exists(path))
                return true;
        }

        return false;
    }

    public override IEnumerable<PathString> GetFiles(PathString directory = default, string[]? allowedFileNames = null)
    {
        var paths = new List<PathString>();
        foreach (var package in packages)
        {
            var packageFiles = package.GetFiles(directory, allowedFileNames);
            foreach (var pathString in packageFiles)
            {
                if (paths.Any(x => x.Equals(pathString)))
                {
                    paths.Remove(paths.First(x => x.Equals(pathString)));
                }

                paths.Add(pathString);
            }
        }

        return paths;
    }

    public override PathString GetFullFilePath(PathString path)
    {
        throw new NotImplementedException();
    }

    public override DateTime? TryGetLastWriteTimeUtc(PathString path)
    {
        foreach (var package in packages)
        {
            if (package.TryGetLastWriteTimeUtc(path).HasValue)
                return package.TryGetLastWriteTimeUtc(path);
        }

        return null;
    }

    public override byte[] ReadBytes(PathString path)
    {
        foreach (var package in packages)
        {
            var data = package.ReadBytes(path);
            if (data != null)
                return data;
        }

        return null;
    }

    public override Stream ReadStream(PathString path)
    {
        foreach (var package in packages)
        {
            var data = package.ReadStream(path);
            if (data != null)
                return data;
        }

        return null;
    }

    public override PathString? TryGetPhysicalPath(PathString path)
    {
        foreach (var package in packages)
        {
            var attempPath = package.TryGetPhysicalPath(path);
            if (attempPath != null)
                return attempPath;
        }

        return null;
    }
}
