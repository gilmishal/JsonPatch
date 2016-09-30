// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public interface IAdapter
    {
        object Get();

        void Add(object value);

        void Remove();

        void Replace(object value);
    }
}
