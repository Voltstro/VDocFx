// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

/// <summary>
/// Holds a collection of deduped errors with a maximum bound.
/// </summary>
internal class ErrorSink
{
    private const int MaxRemoveDeduplicationLogCount = 300;

    private readonly HashSet<Error> _errors = new();

    private bool _exceedMax;

    public int ErrorCount { get; private set; }

    public int WarningCount { get; private set; }

    public int SuggestionCount { get; private set; }

    public int InfoCount { get; private set; }

    public ErrorSinkResult Add(Config? config, Error error)
    {
        lock (_errors)
        {
            var exceedMaxAllowed = config != null && error.Level switch
            {
                ErrorLevel.Error => ErrorCount >= config.MaxFileErrors,
                ErrorLevel.Warning => WarningCount >= config.MaxFileWarnings,
                ErrorLevel.Suggestion => SuggestionCount >= config.MaxFileSuggestions,
                ErrorLevel.Info => InfoCount >= config.MaxFileInfos,
                _ => false,
            };

            if (exceedMaxAllowed)
            {
                if (!_exceedMax)
                {
                    _exceedMax = true;
                    return ErrorSinkResult.Exceed;
                }

                return ErrorSinkResult.Ignore;
            }

            if (!_errors.Add(error with { MessageArguments = Array.Empty<string>() }))
            {
                return ErrorSinkResult.Ignore;
            }

            return ErrorSinkResult.Ok;
        }
    }
}
