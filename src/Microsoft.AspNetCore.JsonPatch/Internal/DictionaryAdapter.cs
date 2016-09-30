// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    //todo 1: behavior when the instance is a readonly dictionary
    //todo 2: behavior when the instance is a custom dictionary not implementing IDictionary
    public class DictionaryAdapter : IAdapter
    {
        private readonly string _propertyName;
        private readonly Operation _operation;
        private readonly IDictionary _dictionary;

        public DictionaryAdapter(IDictionary dictionary, string propertyName, Operation operation)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _dictionary = dictionary;
            _propertyName = propertyName;
            _operation = operation;
        }

        public void Add(object value)
        {
            // As per JsonPatch spec, if a key already exists, adding should replace the existing value
            _dictionary[_propertyName] = value;
        }

        public object Get()
        {
            return _dictionary[_propertyName];
        }

        public void Remove()
        {
            // As per JsonPatch spec, the target location must exist for remove to be successful
            VerifyKeyExists();

            _dictionary.Remove(_propertyName);
        }

        public void Replace(object value)
        {
            // As per JsonPatch spec, the target location must exist for replace to be successful
            VerifyKeyExists();

            _dictionary[_propertyName] = value;
        }

        private void VerifyKeyExists()
        {
            if (!_dictionary.Contains(_propertyName))
            {
                throw new JsonPatchException(new JsonPatchError(
                    _dictionary,
                    _operation,
                    Resources.FormatTargetLocationNotFound(_operation.op, _operation.path)));
            }
        }
    }
}
