// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public interface IAdapter
    {
        bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message);

        bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value);
    }
}
