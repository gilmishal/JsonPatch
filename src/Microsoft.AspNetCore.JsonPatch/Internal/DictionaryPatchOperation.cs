// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal class DictionaryPatchOperation : IPatchOperation
    {
        private readonly IDictionaryWrapper _dictionaryWrapper;
        private readonly string _propertyName;
        private readonly Action<JsonPatchError> _logError;
        private readonly Operation _operation;
        private readonly string _path;

        public DictionaryPatchOperation(
            IDictionaryWrapper targetObject,
            string propertyName,
            Action<JsonPatchError> logErrorAction,
            string path,
            Operation operation)
        {
            _dictionaryWrapper = targetObject;
            _propertyName = propertyName;
            _logError = logErrorAction;
            _path = path;
            _operation = operation;
        }

        public void Add(object value)
        {
            _dictionaryWrapper.SetValue(_propertyName, value);
        }

        public object Get()
        {
            return _dictionaryWrapper.GetValue(_propertyName);
        }

        public void Remove()
        {
            if (!_dictionaryWrapper.ContainsKey(_propertyName))
            {
                LogError(Resources.FormatCannotPerformOperation("remove", _path));
            }
            _dictionaryWrapper.RemoveValue(_propertyName);
        }

        public void Replace(object value)
        {
            _dictionaryWrapper.SetValue(_propertyName, value);
        }

        private void LogError(string message)
        {
            var jsonPatchError = new JsonPatchError(_dictionaryWrapper.WrappedObject, _operation, message);

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
