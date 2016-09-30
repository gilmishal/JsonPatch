// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.AspNetCore.JsonPatch.Exceptions;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ListVisitor : IVisitor
    {
        public IAdapter GetAdapter(OperationContext context)
        {
            var list = context.TargetObject as IList;
            if (list == null)
            {
                return null;
            }

            if (context.CurrentSegment.IsFinal)
            {
                return new ListAdapter(list, context.CurrentSegment, context.Operation);
            }
            else
            {
                int index = -1;
                if (!int.TryParse(context.CurrentSegment, out index))
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
                return ObjectVisitor.GetAdapter(context);
            }
        }
    }
}
