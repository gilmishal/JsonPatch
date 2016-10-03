// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ObjectVisitor2
    {
        public ObjectVisitor2(ParsedPath path, IContractResolver contractResolver)
        {
            Path = path;
            ContractResolver = contractResolver;
        }

        public IContractResolver ContractResolver { get; }

        public ParsedPath Path { get; }

        public bool Visit(ref object target, out IAdapter adapter)
        {
            if (target == null)
            {
                adapter = null;
                return false;
            }

            adapter = SelectAdapater(target);

            for (var i = 0; i < Path.Segments.Count - 1; i++)
            {
                object next;
                if (!adapter.TryTraverse(target, Path.Segments[i], ContractResolver, out next))
                {
                    adapter = null;
                    return false;
                }

                target = next;
                adapter = SelectAdapater(target);
            }

            return true;
        }

        private IAdapter SelectAdapater(object targetObject)
        {
            if (targetObject is ExpandoObject)
            {
                return new ExpandoObjectAdapter();
            }
            else if (targetObject is IDictionary)
            {
                return new DictionaryAdapter();
            }
            else if (targetObject is IList)
            {
                return new ListAdapter();
            }
            else
            {
                return new PocoAdapter();
            }
        }

        private class DictionaryAdapter : IAdapter
        {
            public bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var dictionary = (IDictionary)target;

                // As per JsonPatch spec, if a key already exists, adding should replace the existing value
                dictionary[segment] = value;

                message = null;
                return true;
            }

            public bool TryGet(object target, string segment, IContractResolver contractResolver, out object value, out string message)
            {
                var dictionary = (IDictionary)target;
                value = dictionary[segment];

                message = null;
                return true;
            }

            public bool TryRemove(object target, string segment, IContractResolver contractResolver, out string message)
            {
                var dictionary = (IDictionary)target;
                // As per JsonPatch spec, the target location must exist for remove to be successful
                if (!dictionary.Contains(segment))
                {
                    message = Resources.FormatTargetLocationNotFound("Remove", segment);
                    return false;
                }

                dictionary.Remove(segment);
                message = null;
                return true;
            }

            public bool TryReplace(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var dictionary = (IDictionary)target;
                // As per JsonPatch spec, the target location must exist for remove to be successful
                if (!dictionary.Contains(segment))
                {
                    message = Resources.FormatTargetLocationNotFound("Replace", segment);
                    return false;
                }

                message = null;
                dictionary[segment] = value;
                return true;
            }

            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                var dictionary = target as IDictionary;
                if (dictionary == null)
                {
                    value = null;
                    return false;
                }

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

        private class ExpandoObjectAdapter : IAdapter
        {
            public bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var dictionary = (IDictionary<string, object>)target;
                var key = dictionary.GetKeyUsingCaseInsensitiveSearch(segment);

                // As per JsonPatch spec, if a key already exists, adding should replace the existing value
                dictionary[key] = ConvertValue(dictionary, key, value);

                message = null;
                return true;
            }

            public bool TryGet(object target, string segment, IContractResolver contractResolver, out object value, out string message)
            {
                var dictionary = (IDictionary<string, object>)target;
                var key = dictionary.GetKeyUsingCaseInsensitiveSearch(segment);
                value = dictionary[key];

                message = null;
                return true;
            }

            public bool TryRemove(object target, string segment, IContractResolver contractResolver, out string message)
            {
                var dictionary = (IDictionary<string, object>)target;
                var key = dictionary.GetKeyUsingCaseInsensitiveSearch(segment);
                // As per JsonPatch spec, the target location must exist for remove to be successful
                if (!dictionary.ContainsKey(key))
                {
                    message = Resources.FormatTargetLocationNotFound("Remove", segment);
                    return false;
                }

                message = null;
                dictionary.Remove(key);
                return true;
            }

            public bool TryReplace(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var dictionary = (IDictionary<string, object>)target;
                var key = dictionary.GetKeyUsingCaseInsensitiveSearch(segment);
                // As per JsonPatch spec, the target location must exist for remove to be successful
                if (!dictionary.ContainsKey(key))
                {
                    message = Resources.FormatTargetLocationNotFound("Replace", segment);
                    return false;
                }

                message = null;
                dictionary[key] = ConvertValue(dictionary, key, value);
                return true;
            }

            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                var dictionary = target as IDictionary<string, object>;
                if (dictionary == null)
                {
                    value = null;
                    return false;
                }

                var key = dictionary.GetKeyUsingCaseInsensitiveSearch(segment);
                if (dictionary.ContainsKey(key))
                {
                    value = dictionary[key];
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            private object ConvertValue(IDictionary<string, object> dictionary, string key, object newValue)
            {
                object existingValue = null;
                if (dictionary.TryGetValue(key, out existingValue))
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

        private class ListAdapter : IAdapter
        {
            public bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var list = (IList)target;

                Type typeArgument = null;
                if (!TryGetListTypeArgument(list, out typeArgument, out message))
                {
                    return false;
                }

                var positionInfo = GetPositionInfo(list, segment);
                if (!TryValidatePosition(positionInfo, out message))
                {
                    return false;
                }

                object convertedValue = null;
                if (!TryConvertValue(value, typeArgument, out convertedValue, out message))
                {
                    return false;
                }

                if (positionInfo.Type == PositionType.EndOfList)
                {
                    list.Add(convertedValue);
                }
                else
                {
                    list.Insert(positionInfo.Index, convertedValue);
                }

                message = null;
                return true;
            }

            public bool TryGet(object target, string segment, IContractResolver contractResolver, out object value, out string message)
            {
                var list = (IList)target;

                Type typeArgument = null;
                if (!TryGetListTypeArgument(list, out typeArgument, out message))
                {
                    value = null;
                    return false;
                }

                var positionInfo = GetPositionInfo(list, segment);
                if (!TryValidatePosition(positionInfo, out message))
                {
                    value = null;
                    return false;
                }

                if (positionInfo.Type == PositionType.EndOfList)
                {
                    value = list[list.Count - 1];
                }
                else
                {
                    value = list[positionInfo.Index];
                }

                message = null;
                return true;
            }

            public bool TryRemove(object target, string segment, IContractResolver contractResolver, out string message)
            {
                var list = (IList)target;

                Type typeArgument = null;
                if (!TryGetListTypeArgument(list, out typeArgument, out message))
                {
                    return false;
                }

                var positionInfo = GetPositionInfo(list, segment);
                if (!TryValidatePosition(positionInfo, out message))
                {
                    return false;
                }

                if (positionInfo.Type == PositionType.EndOfList)
                {
                    list.RemoveAt(list.Count - 1);
                }
                else
                {
                    list.RemoveAt(positionInfo.Index);
                }

                message = null;
                return true;
            }

            public bool TryReplace(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                var list = (IList)target;

                Type typeArgument = null;
                if (!TryGetListTypeArgument(list, out typeArgument, out message))
                {
                    return false;
                }

                var positionInfo = GetPositionInfo(list, segment);
                if (!TryValidatePosition(positionInfo, out message))
                {
                    return false;
                }

                object convertedValue = null;
                if (!TryConvertValue(value, typeArgument, out convertedValue, out message))
                {
                    return false;
                }

                if (positionInfo.Type == PositionType.EndOfList)
                {
                    list[list.Count - 1] = convertedValue;
                }
                else
                {
                    list[positionInfo.Index] = convertedValue;
                }

                message = null;
                return true;
            }

            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                var list = target as IList;
                if (list == null)
                {
                    value = null;
                    return false;
                }

                int index = -1;
                if (!int.TryParse(segment, out index))
                {
                    value = null;
                    throw new InvalidOperationException("Invalid value for array property");
                }

                if (index < 0 || index >= list.Count)
                {
                    value = null;
                    throw new InvalidOperationException("Invalid index for array property");
                }

                value = list[index];
                return true;
            }

            private bool TryConvertValue(object originalValue, Type listTypeArgument, out object convertedValue, out string message)
            {
                var conversionResult = ConversionResultProvider.ConvertTo(originalValue, listTypeArgument);
                if (!conversionResult.CanBeConverted)
                {
                    convertedValue = null;
                    message = "Invalid value for property";
                    return false;
                }

                convertedValue = conversionResult.ConvertedInstance;
                message = null;
                return true;
            }

            private bool TryGetListTypeArgument(IList list, out Type listTypeArgument, out string message)
            {
                // Arrays are not supported as they have fixed size and operations like Add, Insert do not make sense
                var listType = list.GetType();
                if (listType.IsArray)
                {
                    message = "Patch not supported for arrays";
                    listTypeArgument = null;
                    return false;
                }
                else
                {
                    var genericList = ClosedGenericMatcher.ExtractGenericInterface(listType, typeof(IList<>));
                    if (genericList == null)
                    {
                        message = "Patch not supported for non-generic lists";
                        listTypeArgument = null;
                        return false;
                    }
                    else
                    {
                        message = null;
                        listTypeArgument = genericList.GenericTypeArguments[0];
                        return true;
                    }
                }
            }

            private bool TryValidatePosition(PositionInfo positionInfo, out string message)
            {
                if (positionInfo.Type == PositionType.Invalid)
                {
                    message = "Invalid value for array property";
                    return false;
                }
                else if (positionInfo.Type == PositionType.OutOfBounds)
                {
                    message = "Invalid index for array property";
                    return false;
                }
                else
                {
                    message = null;
                    return true;
                }
            }

            private PositionInfo GetPositionInfo(IList list, string segment)
            {
                if (segment == "-")
                {
                    return new PositionInfo(PositionType.EndOfList, -1);
                }

                int position = -1;
                if (int.TryParse(segment, out position))
                {
                    if (position >= 0 && position < list.Count)
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

        private class PocoAdapter : IAdapter
        {
            public bool TryAdd(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                JsonProperty jsonProperty = null;
                if (!TryGetJsonProperty(target, contractResolver, segment, out jsonProperty))
                {
                    message = "Property not found";
                    return false;
                }

                if (!jsonProperty.Writable)
                {
                    message = "Property not writable";
                    return false;
                }

                object convertedValue = null;
                if (!TryConvertValue(value, jsonProperty.PropertyType, out convertedValue))
                {
                    message = "invalid value for property";
                    return false;
                }

                jsonProperty.ValueProvider.SetValue(target, convertedValue);

                message = null;
                return true;
            }

            public bool TryGet(object target, string segment, IContractResolver contractResolver, out object value, out string message)
            {
                JsonProperty jsonProperty = null;
                if (!TryGetJsonProperty(target, contractResolver, segment, out jsonProperty))
                {
                    message = "Property not found";
                    value = null;
                    return false;
                }

                if (!jsonProperty.Readable)
                {
                    message = "Property not readable";
                    value = null;
                    return false;
                }

                value = jsonProperty.ValueProvider.GetValue(target);
                message = null;
                return true;
            }

            public bool TryRemove(object target, string segment, IContractResolver contractResolver, out string message)
            {
                JsonProperty jsonProperty = null;
                if (!TryGetJsonProperty(target, contractResolver, segment, out jsonProperty))
                {
                    message = "Property not found";
                    return false;
                }

                if (!jsonProperty.Writable)
                {
                    message = "Property not writable";
                    return false;
                }

                // setting the value to "null" will use the default value in case of value types, and
                // null in case of reference types
                object value = null;
                if (jsonProperty.PropertyType.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(jsonProperty.PropertyType) == null)
                {
                    value = Activator.CreateInstance(jsonProperty.PropertyType);
                }
                jsonProperty.ValueProvider.SetValue(target, value);

                message = null;
                return true;
            }

            public bool TryReplace(object target, string segment, IContractResolver contractResolver, object value, out string message)
            {
                JsonProperty jsonProperty = null;
                if (!TryGetJsonProperty(target, contractResolver, segment, out jsonProperty))
                {
                    message = "Property not found";
                    return false;
                }

                if (!jsonProperty.Writable)
                {
                    message = "Property not writable";
                    return false;
                }

                object convertedValue = null;
                if (!TryConvertValue(value, jsonProperty.PropertyType, out convertedValue))
                {
                    message = "invalid value for property";
                    return false;
                }

                jsonProperty.ValueProvider.SetValue(target, convertedValue);

                message = null;
                return true;
            }

            public bool TryTraverse(object target, string segment, IContractResolver contractResolver, out object value)
            {
                if (target == null)
                {
                    value = null;
                    return false;
                }

                JsonProperty jsonProperty = null;
                if (TryGetJsonProperty(target, contractResolver, segment, out jsonProperty))
                {
                    value = jsonProperty.ValueProvider.GetValue(target);
                    return true;
                }

                value = null;
                return false;
            }

            private bool TryGetJsonProperty(object target, IContractResolver contractResolver, string segment, out JsonProperty jsonProperty)
            {
                var jsonObjectContract = contractResolver.ResolveContract(target.GetType()) as JsonObjectContract;
                if (jsonObjectContract != null)
                {
                    var pocoProperty = jsonObjectContract
                        .Properties
                        .FirstOrDefault(p => string.Equals(p.PropertyName, segment, StringComparison.OrdinalIgnoreCase));

                    if (pocoProperty != null)
                    {
                        jsonProperty = pocoProperty;
                        return true;
                    }
                }

                jsonProperty = null;
                return false;
            }

            private bool TryConvertValue(object value, Type propertyType, out object convertedValue)
            {
                var conversionResult = ConversionResultProvider.ConvertTo(value, propertyType);
                if (!conversionResult.CanBeConverted)
                {
                    convertedValue = null;
                    return false;
                }

                convertedValue = conversionResult.ConvertedInstance;
                return true;
            }
        }
    }
}
