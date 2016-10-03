// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Dynamic;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ObjectVisitor
    {
        public ObjectVisitor(ParsedPath path, IContractResolver contractResolver)
        {
            Path = path;
            ContractResolver = contractResolver;
        }

        public IContractResolver ContractResolver { get; }

        public ParsedPath Path { get; }

        public bool Visit(ref object target, out IAdapter adapter)
        {
            if (target == null)
            {
                adapter = null;
                return false;
            }

            adapter = SelectAdapater(target);

            for (var i = 0; i < Path.Segments.Count - 1; i++)
            {
                object next;
                if (!adapter.TryTraverse(target, Path.Segments[i], ContractResolver, out next))
                {
                    adapter = null;
                    return false;
                }

                adapter = SelectAdapater(target);
            }

            return true;
        }

        private IAdapter SelectAdapater(object @object)
        {
            if (@object is ExpandoObject)
            {
                return new ExpandoObjectAdapter();
            }
            else if (@object is IDictionary)
            {
                return new DictionaryAdapter();
            }
            else if (@object is IList)
            {
                return new ListAdapter();
            }
            else
            {
                return new PocoAdapter();
            }
        }

        private class ExpandoObjectAdapter : IAdapter
        {
            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                throw new NotImplementedException();
            }
        }

        private class ListAdapter : IAdapter
        {
            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                throw new NotImplementedException();
            }
        }

        private class PocoAdapter : IAdapter
        {
            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
