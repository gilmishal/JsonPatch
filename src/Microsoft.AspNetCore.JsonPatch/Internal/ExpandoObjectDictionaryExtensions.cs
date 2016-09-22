// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    // Helper methods to allow case-insensitive key search
    public static class ExpandoObjectDictionaryExtensions
    {
        internal static void SetValueForCaseInsensitiveKey(
            this IDictionary<string, object> propertyDictionary,
            string key,
            object value)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    propertyDictionary[kvp.Key] = value;
                    break;
                }
            }
        }

        internal static void RemoveValueForCaseInsensitiveKey(
            this IDictionary<string, object> propertyDictionary,
            string key)
        {
            string realKey = null;
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    realKey = kvp.Key;
                    break;
                }
            }

            if (realKey != null)
            {
                propertyDictionary.Remove(realKey);
            }
        }

        internal static object GetValueForCaseInsensitiveKey(
            this IDictionary<string, object> propertyDictionary,
            string key)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            throw new ArgumentException(Resources.FormatDictionaryKeyNotFound(key));
        }

        internal static bool ContainsCaseInsensitiveKey(
            this IDictionary<string, object> propertyDictionary,
            string key)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal static string GetKeyUsingCaseInsensitiveSearch(
            this IDictionary<string, object> propertyDictionary,
            string key)
        {
            foreach (var keyInDictionary in propertyDictionary.Keys)
            {
                if (string.Equals(key, keyInDictionary, StringComparison.OrdinalIgnoreCase))
                {
                    return keyInDictionary;
                }
            }
            return key;
        }
    }
}