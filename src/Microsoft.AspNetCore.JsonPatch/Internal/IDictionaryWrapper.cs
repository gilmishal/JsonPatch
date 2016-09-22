// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal interface IDictionaryWrapper
    {
        object WrappedObject { get; }
        object GetValue(object key);
        void SetValue(object key, object value);
        void RemoveValue(object key);
        bool ContainsKey(object key);
    }
}
