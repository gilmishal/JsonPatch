// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Dynamic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ObjectVisitor
    {
        public static IPatchObject Visit(JsonPatchContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.TargetObject == null)
            {
                throw new JsonPatchException(new JsonPatchError(
                        context.TargetObject,
                        context.Operation,
                        Resources.FormatTargetLocationNotFound(context.Operation.op, context.Operation.path)));
            }

            ExpandoObject expandoObject = null;
            IDictionary dictionary = null;
            IList list = null;
            IPatchObject patchObject = null;

            if ((expandoObject = context.TargetObject as ExpandoObject) != null)
            {
                patchObject = ExpandoObjectVisitor.Visit(context);
            }
            else if ((dictionary = context.TargetObject as IDictionary) != null)
            {
                patchObject = DictionaryObjectVisitor.Visit(context);
            }
            else if ((list = context.TargetObject as IList) != null)
            {
                patchObject = ListObjectVisitor.Visit(context);
            }
            else
            {
                patchObject = PocoObjectVisitor.Visit(context);
            }

            if (patchObject == null)
            {
                throw new JsonPatchException(new JsonPatchError(
                    context.TargetObject,
                    context.Operation,
                    Resources.FormatCannotPerformOperation(context.Operation.op, context.Operation.path)));
            }

            return patchObject;
        }
    }
}
