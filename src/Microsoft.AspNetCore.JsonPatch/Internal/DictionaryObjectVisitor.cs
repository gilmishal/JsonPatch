// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class DictionaryObjectVisitor
    {
        public static IPatchObject Visit(OperationContext context)
        {
            PathSegment pathSegment;
            if (!context.TryGetSegment(out pathSegment))
            {
                return null;
            }

            var _dictionary = (IDictionary)context.TargetObject;

            // Since PathSegment is struct and the dictionary's key is of type 'object', convert it explicitly to
            // string, otherwise look up will fail.
            var key = pathSegment.ToString();

            // Example: /USStatesProperty/WA
            if (pathSegment.IsFinal)
            {
                return new PatchDictionaryObject(_dictionary, pathSegment, context.Operation);
            }
            else if (_dictionary.Contains(key))
            {
                // Example path: "/Customers/101/Address/Zipcode" and
                // let's say the current path segment is "101"
                var newTargetObject = _dictionary[key];
                context.SetNewTargetObject(newTargetObject);
                return ObjectVisitor.Visit(context);
            }

            return null;
        }
    }
}