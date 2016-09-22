// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal class InvalidResult : IPatchOperation
    {
        public void Add(object value)
        {
            throw new NotImplementedException();
        }

        public object Get()
        {
            throw new NotImplementedException();
        }

        public void Move(object fromKey, object toKey)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void Replace(object value)
        {
            throw new NotImplementedException();
        }
    }
}
