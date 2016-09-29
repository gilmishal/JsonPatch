// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class PocoVisitor
    {
        public static IPatchObject Visit(OperationContext context)
        {
            PathSegment pathSegment;
            if (!context.TryGetSegment(out pathSegment))
            {
                return null;
            }

            var jsonObjectContract = context.ContractResolver.ResolveContract(context.TargetObject.GetType()) as JsonObjectContract;
            if (jsonObjectContract != null)
            {
                var pocoProperty = jsonObjectContract
                    .Properties
                    .FirstOrDefault(p => string.Equals(p.PropertyName, pathSegment, StringComparison.OrdinalIgnoreCase));

                if (pocoProperty != null)
                {
                    if (pathSegment.IsFinal)
                    {
                        return new PatchPocoObject(context.TargetObject, pocoProperty, context.Operation);
                    }
                    else
                    {
                        var newTargetObject = pocoProperty.ValueProvider.GetValue(context.TargetObject);
                        context.SetNewTargetObject(newTargetObject);
                        return ObjectVisitor.Visit(context);
                    }
                }
            }

            return null;
        }
    }
}
