// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.AspNetCore.JsonPatch.Exceptions;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class DictionaryVisitor : IVisitor
    {
        public IAdapter GetAdapter(OperationContext context)
        {
            var dictionary = context.TargetObject as IDictionary;
            if (dictionary == null)
            {
                return null;
            }

            var currentSegment = context.CurrentSegment.ToString();

            // Example: /USStatesProperty/WA
            if (context.CurrentSegment.IsFinal)
            {
                return new DictionaryAdapter(dictionary, currentSegment, context.Operation);
            }
            else if (dictionary.Contains(currentSegment))
            {
                // Example path: "/Customers/101/Address/Zipcode" and
                // let's say the current path segment is "101"
                var newTargetObject = dictionary[currentSegment];
                context.SetNewTargetObject(newTargetObject);
                return ObjectVisitor.GetAdapter(context);
            }

            throw new JsonPatchException(new JsonPatchError(
                context.TargetObject,
                context.Operation,
                Resources.FormatCannotPerformOperation(context.Operation.op, context.Operation.path)));
        }
    }
}
