using Microsoft.Docs.Build;
using Newtonsoft.Json.Linq;

namespace VDocFx.Api;

/// <summary>
///     Standard interface for API builders
/// </summary>
internal interface IApiBuilder
{
    /// <summary>
    ///     Build API docs
    /// </summary>
    /// <param name="config"></param>
    /// <param name="errors"></param>
    /// <param name="mainDirectory"></param>
    /// <param name="outputDirectory"></param>
    public void Build(JToken config, ErrorBuilder errors, PathString mainDirectory, PathString outputDirectory);
}
