// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal interface ICacheObject<TKey> where TKey : notnull
{
    DateTime? UpdatedAt { get; set; }

    IEnumerable<TKey> GetKeys();
}
