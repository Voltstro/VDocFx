// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Docs.Build;

internal class JoinTOCConfig
{
    public PathString OutputFolder { get; init; }

    public JObject? ContainerPageMetadata { get; init; }

    public string? ReferenceToc { get; init; }

    public string? TopLevelToc { get; init; }

    public string? OriginalReferenceToc { get; init; }
}
