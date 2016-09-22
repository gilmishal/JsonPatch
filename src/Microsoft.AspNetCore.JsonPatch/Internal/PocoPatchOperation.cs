// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal class PocoPatchOperation : IPatchOperation
    {
        private readonly Action<JsonPatchError> _logError;
        private readonly Operation _operation;
        private readonly string _path;
        private readonly JsonProperty _property;
        private readonly string _propertyName;
        private readonly object _targetObject;

        public PocoPatchOperation(
            object targetObject,
            string propertyName,
            JsonProperty property,
            Action<JsonPatchError> logErrorAction,
            string path,
            Operation operation)
        {
            _targetObject = targetObject;
            _propertyName = propertyName;
            _property = property;
            _logError = logErrorAction;
            _path = path;
            _operation = operation;
        }

        public void Add(object value)
        {
            var conversionResult = ResultHelper.ConvertObjectToType(value, _property.PropertyType);
            if (!conversionResult.CanBeConverted)
            {
                LogError(Resources.FormatInvalidValueForProperty(value, _path));
                return;
            }

            if (!_property.Writable)
            {
                LogError(Resources.FormatCannotUpdateProperty(_path));
                return;
            }

            _property.ValueProvider.SetValue(_targetObject, conversionResult.ConvertedInstance);
        }

        public object Get()
        {
            if (!_property.Readable)
            {
                LogError(Resources.FormatCannotReadProperty(_path));
            }

            return _property.ValueProvider.GetValue(_targetObject);
        }

        public void Remove()
        {
            if (!_property.Writable)
            {
                LogError(Resources.FormatCannotUpdateProperty(_path));
                return;
            }

            // setting the value to "null" will use the default value in case of value types, and
            // null in case of reference types
            object value = null;
            if (_property.PropertyType.GetTypeInfo().IsValueType
                && Nullable.GetUnderlyingType(_property.PropertyType) == null)
            {
                value = Activator.CreateInstance(_property.PropertyType);
            }
            _property.ValueProvider.SetValue(_targetObject, value);
        }

        public void Replace(object value)
        {
            var conversionResult = ResultHelper.ConvertObjectToType(value, _property.PropertyType);
            if (!conversionResult.CanBeConverted)
            {
                LogError(Resources.FormatInvalidValueForProperty(value, _path));
                return;
            }

            _property.ValueProvider.SetValue(_targetObject, conversionResult.ConvertedInstance);
        }

        private void LogError(string message)
        {
            var jsonPatchError = new JsonPatchError(_targetObject, _operation, message);

            if (_logError != null)
            {
                _logError(jsonPatchError);
            }
            else
            {
                throw new JsonPatchException(jsonPatchError);
            }
        }
    }
}
