// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal static class ResultHelper
    {
        public static Type IsDictionary(object targetObject)
        {
            return targetObject.GetType().GetTypeInfo().ImplementedInterfaces
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .FirstOrDefault();
        }

        public static ConversionResult ConvertObjectToType(object value, Type typeToConvertTo)
        {
            try
            {
                var o = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), typeToConvertTo);

                return new ConversionResult(true, o);
            }
            catch (Exception)
            {
                return new ConversionResult(false, null);
            }
        }

        public static object GetElementAtFromObject(object targetObject, int numericValue)
        {
            if (numericValue > -1)
            {
                // Check if the targetobject is an IEnumerable,
                // and if the position is valid.
                if (targetObject is IEnumerable)
                {
                    var indexable = ((IEnumerable)targetObject).Cast<object>();

                    if (indexable.Count() >= numericValue)
                    {
                        return indexable.ElementAt(numericValue);
                    }
                    else { return null; }
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
    }
}
