// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal class DictionaryWrapper<TKey, TValue> : IDictionaryWrapper
    {
        private readonly IDictionary<TKey, TValue> _targetDictionary;
        private readonly bool useDynamic;

        public DictionaryWrapper(IDictionary<TKey, TValue> targetDictionary)
        {
            _targetDictionary = targetDictionary;
            WrappedObject = targetDictionary;
            KeyType = typeof(TKey);
            ValueType = typeof(TValue);

            useDynamic = (KeyType == typeof(string)) && (ValueType == typeof(object));
        }

        public Type KeyType { get; }

        public Type ValueType { get; }

        // for logging purpose (jsonpatcherror)
        public object WrappedObject { get; }

        public object GetValue(object key)
        {
            key = GetKeyUsingCaseInsensitiveSearch(key);
            return _targetDictionary[CastTo<TKey>(key)];
        }

        public void SetValue(object key, object value)
        {
            key = GetKeyUsingCaseInsensitiveSearch(key);

            if (useDynamic && ContainsKey(key))
            {
                var currentValue = GetValue(key);
                var result = ResultHelper.ConvertObjectToType(value, currentValue.GetType());

                _targetDictionary[CastTo<TKey>(key)] = CastTo<TValue>(result.ConvertedInstance);
                return;
            }

            _targetDictionary[CastTo<TKey>(key)] = CastTo<TValue>(value);
        }

        public void RemoveValue(object key)
        {
            key = GetKeyUsingCaseInsensitiveSearch(key);
            _targetDictionary.Remove(CastTo<TKey>(key));
        }

        public bool ContainsKey(object key)
        {
            key = GetKeyUsingCaseInsensitiveSearch(key);
            return _targetDictionary.ContainsKey(CastTo<TKey>(key));
        }

        private TModel CastTo<TModel>(object model)
        {
            return model is TModel ? (TModel)model : default(TModel);
        }

        private object GetKeyUsingCaseInsensitiveSearch(object key)
        {
            // Example: a Guid key
            if (KeyType != typeof(string))
            {
                return JsonConvert.DeserializeObject<TKey>(JsonConvert.SerializeObject(key));
            }

            var keyToFind = (string)key;
            foreach (var currentKey in _targetDictionary.Keys)
            {
                var keyInDictionary = CastTo<string>(currentKey);
                if (string.Equals(keyToFind, keyInDictionary, StringComparison.OrdinalIgnoreCase))
                {
                    return keyInDictionary;
                }
            }
            return keyToFind;
        }
    }
}
