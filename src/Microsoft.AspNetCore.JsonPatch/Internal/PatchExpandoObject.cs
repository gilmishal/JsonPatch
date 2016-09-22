using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class PatchExpandoObject : IPatchObject
    {
        private readonly Operation _operation;
        private readonly IDictionary<string, object> _dictionary;
        private readonly string _key;

        public PatchExpandoObject(ExpandoObject targetObject, string propertyName, Operation operation)
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
            object currentValue = null;
            if (_dictionary.TryGetValue(_key, out currentValue))
            {
                if (currentValue != null)
                {
                    var currentValueType = currentValue.GetType();
                    var conversionResult = ConversionResultProvider.ConvertTo(value, currentValueType);
                    if (conversionResult.CanBeConverted)
                    {
                        value = conversionResult.ConvertedInstance;
                    }
                }
            }

            // As per JsonPatch spec, if a key already exists Adding should replace the existing value
            _dictionary[_key] = value;
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

            object currentValue = null;
            if (_dictionary.TryGetValue(_key, out currentValue))
            {
                if (currentValue != null)
                {
                    var currentValueType = currentValue.GetType();
                    var conversionResult = ConversionResultProvider.ConvertTo(value, currentValueType);
                    if (conversionResult.CanBeConverted)
                    {
                        value = conversionResult.ConvertedInstance;
                    }
                }
            }
            _dictionary[_key] = value;
        }

        private void VerifyKeyExists()
        {
            if (!_dictionary.ContainsKey(_key))
            {
                throw new JsonPatchException(new JsonPatchError(_dictionary, _operation, Resources.FormatTargetLocationNotFound(_operation.op, _operation.path)));
            }
        }
    }
}
