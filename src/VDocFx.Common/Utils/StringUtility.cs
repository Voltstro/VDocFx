// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Web;

namespace Microsoft.Docs.Build;

internal static class StringUtility
{
    public static string Html(FormattableString htmlFormat)
    {
        var encodedArgs = Array.ConvertAll(htmlFormat.GetArguments(), arg => arg?.ToString() is string str ? HttpUtility.HtmlEncode(str) : null);
        return string.Format(htmlFormat.Format, encodedArgs);
    }

    public static string ToCamelCase(char wordSeparator, string value)
    {
        var words = value.ToLowerInvariant().Split(wordSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 0)
        {
            return "";
        }

        var sb = new StringBuilder();
        sb.Append(words[0]);
        for (var i = 1; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                sb.Append(char.ToUpperInvariant(words[i][0]));
                sb.Append(words[i], 1, words[i].Length - 1);
            }
        }
        return sb.ToString().Trim();
    }

    public static string UpperCaseFirstChar(string value)
    {
        return value.Length == 0
                    ? value
                    : value.First().ToString().ToUpperInvariant() + value[1..].ToLowerInvariant();
    }

    public static string Join<T>(IEnumerable<T> source, int take = 5)
    {
        var formatSource = source.Select(item => $"'{item}'").OrderBy(_ => _, StringComparer.Ordinal);
        var result = $"{string.Join(", ", formatSource.Take(take))}{(formatSource.Count() > 5 ? "..." : "")}";
        return string.IsNullOrEmpty(result) ? "''" : result;
    }
}
