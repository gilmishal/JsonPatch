// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ExpandoObjectVisitor
    {
        public static IPatchObject Visit(OperationContext context)
        {
            PathSegment pathSegment;
            if (!context.TryGetSegment(out pathSegment))
            {
                return null;
            }

            var _dictionary = (IDictionary<string, object>)context.TargetObject;

            // Example: /USStatesProperty/WA
            if (pathSegment.IsFinal)
            {
                return new PatchExpandoObject((ExpandoObject)context.TargetObject, pathSegment, context.Operation);
            }
            else if (_dictionary.ContainsCaseInsensitiveKey(pathSegment))
            {
                // Example path: "/Customers/101/Address/Zipcode" and
                // let's say the current path segment is "101"
                var newTargetObject = _dictionary.GetValueForCaseInsensitiveKey(pathSegment);
                context.SetNewTargetObject(newTargetObject);
                return ObjectVisitor.Visit(context);
            }

            return null;
        }
    }
}
