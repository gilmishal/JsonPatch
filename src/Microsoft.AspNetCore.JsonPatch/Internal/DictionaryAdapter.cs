// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class DictionaryAdapter : IAdapter
    {
        public void Add(object target, string segment, IContractResolver contractResolver, object value)
        {
            var dictionary = (IDictionary)target;

            // As per JsonPatch spec, if a key already exists, adding should replace the existing value
            dictionary[segment] = value;
        }

        public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
        {
            var dictionary = (IDictionary)target;

            if (dictionary.Contains(segment))
            {
                value = dictionary[segment];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
