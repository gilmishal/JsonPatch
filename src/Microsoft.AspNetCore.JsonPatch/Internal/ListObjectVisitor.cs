// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.AspNetCore.JsonPatch.Exceptions;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ListObjectVisitor
    {
        public static IPatchObject Visit(JsonPatchContext context)
        {
            // Example: Let's say the paths are "/Countries/0" or "/Countries/-" and we are at 'list' target object
            // returned by 'Countries'. We are trying to get '0' or '-' in this case.
            PathSegment pathSegment;
            if (!context.TryGetSegment(out pathSegment))
            {
                return null;
            }

            var list = (IList)context.TargetObject;

            if (pathSegment.IsFinal)
            {
                return new PatchListObject(list, pathSegment, context.Operation);
            }
            else
            {
                int index = -1;
                if (!int.TryParse(pathSegment, out index))
                {
                    throw new JsonPatchException(new JsonPatchError(
                        context.TargetObject,
                        context.Operation,
                        Resources.FormatInvalidPathForArrayProperty(context.Operation.op, context.Operation.path)));
                }

                if (index < 0 || index >= list.Count)
                {
                    throw new JsonPatchException(new JsonPatchError(
                        context.TargetObject,
                        context.Operation,
                        Resources.FormatInvalidIndexForArrayProperty(context.Operation.op, context.Operation.path)));
                }

                // Example paths: "/Countries/0/States/0", "/Countries/0/States/-"
                var newTargetObject = list[index];
                context.SetNewTargetObject(newTargetObject);
                return ObjectVisitor.Visit(context);
            }
        }
    }
}
