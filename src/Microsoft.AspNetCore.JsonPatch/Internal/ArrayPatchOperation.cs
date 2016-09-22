// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal class ArrayPatchOperation : IPatchOperation
    {
        private readonly IList _list;
        private readonly string _propertyName;
        private readonly object _targetObject;
        private readonly Type _listTypeArgument;
        private readonly Action<JsonPatchError> _logError;
        private readonly Operation _operation;
        private readonly string _path;

        public ArrayPatchOperation(
            object targetObject,
            string propertyName,
            Action<JsonPatchError> logErrorAction,
            string path,
            Operation operation)
        {
            _targetObject = targetObject;
            _propertyName = propertyName;
            _list = (IList)_targetObject;
            _logError = logErrorAction;
            _path = path;
            _operation = operation;

            _listTypeArgument = GetIListTypeArgument(_list.GetType());
        }

        public void Add(object value)
        {
            var conversionResult = ResultHelper.ConvertObjectToType(value, _listTypeArgument);
            if (!conversionResult.CanBeConverted)
            {
                LogError(Resources.FormatInvalidValueForProperty(value, _path));
                return;
            }

            if (_propertyName == "-")
            {
                _list.Add(conversionResult.ConvertedInstance);
            }
            else
            {
                int position = -1;
                if (int.TryParse(_propertyName, out position))
                {
                    if (position == _list.Count)
                    {
                        _list.Add(conversionResult.ConvertedInstance);
                    }
                    else if (position >= 0 && position < _list.Count)
                    {
                        _list.Insert(position, conversionResult.ConvertedInstance);
                    }
                    else
                    {
                        LogError(Resources.FormatInvalidIndexForArrayProperty(_operation.op, _path));
                        return;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot add into array");
                }
            }
        }

        public void Remove()
        {
            if (_propertyName == "-")
            {
                _list.RemoveAt(_list.Count - 1);
            }
            else
            {
                int position = -1;
                if (int.TryParse(_propertyName, out position))
                {
                    if (position >= 0 && position < _list.Count)
                    {
                        _list.RemoveAt(position);
                    }
                    else
                    {
                        LogError(Resources.FormatInvalidIndexForArrayProperty(_operation.op, _path));
                        return;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot remove from array");
                }
            }
        }

        public void Replace(object value)
        {
            Remove();
            Add(value);
        }

        private Type GetIListTypeArgument(Type type)
        {
            if (IsGenericListType(type))
            {
                return type.GetTypeInfo().GenericTypeArguments[0];
            }

            foreach (Type interfaceType in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (IsGenericListType(interfaceType))
                {
                    return interfaceType.GetTypeInfo().GenericTypeArguments[0];
                }
            }

            return null;
        }

        private bool IsGenericListType(Type type)
        {
            if (type.GetTypeInfo().IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return true;
            }

            return false;
        }

        public object Get()
        {
            int position = -1;
            if (int.TryParse(_propertyName, out position))
            {
                return _list[position];
            }

            throw new InvalidOperationException($"Position {position} is out of the range");
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
