// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class PocoVisitor : IVisitor
    {
        public IAdapter GetAdapter(OperationContext context)
        {
            var jsonObjectContract = context.ContractResolver.ResolveContract(context.TargetObject.GetType()) as JsonObjectContract;
            if (jsonObjectContract != null)
            {
                var pocoProperty = jsonObjectContract
                    .Properties
                    .FirstOrDefault(p => string.Equals(p.PropertyName, context.CurrentSegment, StringComparison.OrdinalIgnoreCase));

                if (pocoProperty != null)
                {
                    if (context.CurrentSegment.IsFinal)
                    {
                        return new PocoAdapter(context.TargetObject, pocoProperty, context.Operation);
                    }
                    else
                    {
                        var newTargetObject = pocoProperty.ValueProvider.GetValue(context.TargetObject);
                        context.SetNewTargetObject(newTargetObject);
                        return ObjectVisitor.GetAdapter(context);
                    }
                }
            }

            throw new JsonPatchException(new JsonPatchError(
                            context.TargetObject,
                            context.Operation,
                            Resources.FormatCannotPerformOperation(context.Operation.op, context.Operation.path)));
        }
    }
}
