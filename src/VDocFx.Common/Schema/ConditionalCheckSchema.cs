// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class ConditionalCheckSchema
{
    public JsonSchema? Condition { get; set; }

    public int Value { get; set; }
}
