// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Docs.Build;

/// <summary>
/// Represents a javascript engine for a single thread
/// </summary>
internal abstract class JavaScriptEngine : IDisposable
{
    public abstract JToken Run(string scriptPath, string methodName, JToken arg);

    public static JavaScriptEngine Create(Package package, JObject? global = null)
    {
        return new JintJsEngine(package, global);
    }

    public abstract void Dispose();
}
