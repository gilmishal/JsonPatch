// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public interface IAdapter
    {
        bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value);

        bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message);

        bool TryRemove(object target, string segment, IContractResolver contractResolver, out string message);

        bool TryGet(object target, string segment, IContractResolver contractResolver, out object value, out string message);

        bool TryReplace(object target, string segment, IContractResolver contractResolver, object value, out string message);
    }
}
