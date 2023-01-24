using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Docs.Build;

public sealed class Glob
{
    private static readonly char[] globChars = new char[] { '*', '?', '[', ']' };

    private readonly Matcher _matcher;

    public Glob(string[] includePatterns, string[]? excludePatterns)
    {
        _matcher = new Matcher();
        _matcher.AddIncludePatterns(includePatterns);
        if (excludePatterns != null)
        {
            _matcher.AddExcludePatterns(excludePatterns);
        }
    }

    public string[] GetMatchesInDirectory(string directory)
    {
        var result = _matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(directory)));

        return result.Files.Select(resultFile => resultFile.Path).ToArray();
    }

    public bool IsMatch(string path)
    {
        return _matcher.Match(path).HasMatches;
    }

    public static bool IsGlobString(string str) => str.IndexOfAny(globChars) >= 0;
}
