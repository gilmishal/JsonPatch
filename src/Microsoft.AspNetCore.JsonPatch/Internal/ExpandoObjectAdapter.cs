// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ExpandoObjectAdapter : IAdapter
    {
        private readonly Operation _operation;
        private readonly IDictionary<string, object> _dictionary;
        private readonly string _key;

        public ExpandoObjectAdapter(ExpandoObject targetObject, string propertyName, Operation operation)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _dictionary = targetObject as IDictionary<string, object>;
            _operation = operation;
            _key = _dictionary.GetKeyUsingCaseInsensitiveSearch(propertyName);
        }

        public void Add(object value)
        {
            // As per JsonPatch spec, if a key already exists, adding should replace the existing value
            _dictionary[_key] = ConvertValue(value);
        }

        public object Get()
        {
            return _dictionary[_key];
        }

        public void Remove()
        {
            // As per JsonPatch spec, the target location must exist for remove to be successful
            VerifyKeyExists();

            _dictionary.Remove(_key);
        }

        public void Replace(object value)
        {
            // As per JsonPatch spec, the target location must exist for remove to be successful
            VerifyKeyExists();

            _dictionary[_key] = ConvertValue(value);
        }

        private void VerifyKeyExists()
        {
            if (!_dictionary.ContainsKey(_key))
            {
                throw new JsonPatchException(new JsonPatchError(
                    _dictionary,
                    _operation,
                    Resources.FormatTargetLocationNotFound(_operation.op, _operation.path)));
            }
        }

        private object ConvertValue(object newValue)
        {
            object existingValue = null;
            if (_dictionary.TryGetValue(_key, out existingValue))
            {
                if (existingValue != null)
                {
                    var conversionResult = ConversionResultProvider.ConvertTo(newValue, existingValue.GetType());
                    if (conversionResult.CanBeConverted)
                    {
                        return conversionResult.ConvertedInstance;
                    }
                }
            }
            return newValue;
        }
    }
}
