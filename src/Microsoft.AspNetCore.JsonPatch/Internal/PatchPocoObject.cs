// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class PatchPocoObject : IPatchObject
    {
        private readonly Operation _operation;
        private readonly JsonProperty _property;
        private readonly object _targetObject;

        public PatchPocoObject(object targetObject, JsonProperty property, Operation operation)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _targetObject = targetObject;
            _property = property;
            _operation = operation;
        }

        public void Add(object value)
        {
            EnsureWritableProperty();
            _property.ValueProvider.SetValue(_targetObject, ConvertValue(value));
        }

        public object Get()
        {
            if (!_property.Readable)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _targetObject,
                    _operation,
                    Resources.FormatCannotReadProperty(_operation.path)));
            }
            return _property.ValueProvider.GetValue(_targetObject);
        }

        public void Remove()
        {
            EnsureWritableProperty();

            // setting the value to "null" will use the default value in case of value types, and
            // null in case of reference types
            object value = null;
            if (_property.PropertyType.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(_property.PropertyType) == null)
            {
                value = Activator.CreateInstance(_property.PropertyType);
            }
            _property.ValueProvider.SetValue(_targetObject, value);
        }

        public void Replace(object value)
        {
            EnsureWritableProperty();
            _property.ValueProvider.SetValue(_targetObject, ConvertValue(value));
        }

        private void EnsureWritableProperty()
        {
            if (!_property.Writable)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _targetObject,
                    _operation,
                    Resources.FormatCannotUpdateProperty(_operation.path)));
            }
        }

        private object ConvertValue(object value)
        {
            var conversionResult = ConversionResultProvider.ConvertTo(value, _property.PropertyType);
            if (!conversionResult.CanBeConverted)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _targetObject,
                    _operation,
                    Resources.FormatInvalidValueForProperty(value, _operation.path)));
            }
            return conversionResult.ConvertedInstance;
        }
    }
}
