// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ConversionResultProvider
    {
        public static ConversionResult ConvertTo(object value, Type typeToConvertTo)
        {
            if (value == null)
            {
                var typeInfo = typeToConvertTo.GetTypeInfo();
                if (!IsNullableType(typeInfo) && typeInfo.IsValueType)
                {
                    return new ConversionResult(false, null);
                }
                return new ConversionResult(true, null);
            }

            if (typeToConvertTo.IsAssignableFrom(value.GetType()))
            {
                return new ConversionResult(true, value);
            }

            try
            {
                var jarray = value as JArray;
                if (jarray != null)
                {
                    return new ConversionResult(true, jarray.ToObject(typeToConvertTo));
                }

                var jobject = value as JObject;
                if (jobject != null)
                {
                    return new ConversionResult(true, jobject.ToObject(typeToConvertTo));
                }

                var deserialized = JToken.FromObject(value).ToObject(typeToConvertTo);
                return new ConversionResult(true, deserialized);
            }
            catch
            {
                return new ConversionResult(canBeConverted: false, convertedInstance: null);
            }
        }

        private static bool IsNullableType(TypeInfo typeInfo)
        {
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
