// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class PatchListObject : IPatchObject
    {
        private readonly IList _list;
        private readonly string _pathSegment;
        private readonly Type _genericListTypeArgument;
        private readonly Operation _operation;
        private readonly PositionInfo _positionInfo;

        public PatchListObject(IList targetObject, string pathSegment, Operation operation)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }
            if (pathSegment == null)
            {
                throw new ArgumentNullException(nameof(pathSegment));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _list = targetObject;
            _pathSegment = pathSegment;
            _operation = operation;

            // Arrays are not supported as they have fixed size and operations like Add, Insert do not make sense
            var _listType = _list.GetType();
            if (_listType.IsArray)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _list,
                    _operation,
                    Resources.FormatPatchNotSupportedForArrays(_listType.FullName)));
            }

            var genericList = ClosedGenericMatcher.ExtractGenericInterface(_listType, typeof(IList<>));
            if (genericList == null)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _list,
                    _operation,
                    Resources.FormatPatchNotSupportedForNonGenericLists(_listType.FullName)));
            }
            else
            {
                _genericListTypeArgument = genericList.GenericTypeArguments[0];
            }

            _positionInfo = GetPositionInfo();
            if (_positionInfo.Type == PositionType.Invalid)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _list,
                    _operation,
                    Resources.FormatInvalidPathForArrayProperty(_operation.op, _operation.path)));
            }
            else if (_positionInfo.Type == PositionType.OutOfBounds)
            {
                throw new JsonPatchException(new JsonPatchError(
                    _list,
                    _operation,
                    Resources.FormatInvalidIndexForArrayProperty(_operation.op, _operation.path)));
            }
        }

        public void Add(object value)
        {
            if (_positionInfo.Type == PositionType.EndOfList)
            {
                _list.Add(ConvertValue(value));
            }
            else
            {
                _list.Insert(_positionInfo.Index, ConvertValue(value));
            }
        }

        public void Remove()
        {
            if (_positionInfo.Type == PositionType.EndOfList)
            {
                _list.RemoveAt(_list.Count - 1);
            }
            else
            {
                _list.RemoveAt(_positionInfo.Index);
            }
        }

        public void Replace(object value)
        {
            if (_positionInfo.Type == PositionType.EndOfList)
            {
                _list[_list.Count - 1] = ConvertValue(value);
            }
            else
            {
                _list[_positionInfo.Index] = ConvertValue(value);
            }
        }

        public object Get()
        {
            if (_positionInfo.Type == PositionType.EndOfList)
            {
                return _list[_list.Count - 1];
            }
            else
            {
                return _list[_positionInfo.Index];
            }
        }

        private object ConvertValue(object value)
        {
            var conversionResult = ConversionResultProvider.ConvertTo(value, _genericListTypeArgument);
            if (!conversionResult.CanBeConverted)
            {
                throw new JsonPatchException(new JsonPatchError(_list, _operation, Resources.FormatInvalidValueForProperty(value, _operation.path)));
            }
            return conversionResult.ConvertedInstance;
        }

        private PositionInfo GetPositionInfo()
        {
            if (_pathSegment == "-")
            {
                return new PositionInfo(PositionType.EndOfList, -1);
            }

            int position = -1;
            if (int.TryParse(_pathSegment, out position))
            {
                if (position >= 0 && position < _list.Count)
                {
                    return new PositionInfo(PositionType.Index, position);
                }
                else
                {
                    return new PositionInfo(PositionType.OutOfBounds, position);
                }
            }
            else
            {
                return new PositionInfo(PositionType.Invalid, -1);
            }
        }

        private struct PositionInfo
        {
            public PositionInfo(PositionType type, int index)
            {
                Type = type;
                Index = index;
            }

            public PositionType Type { get; }
            public int Index { get; }
        }

        private enum PositionType
        {
            Index, // valid index
            EndOfList, // '-'
            Invalid, // Ex: not an integer
            OutOfBounds
        }
    }
}
