// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ObjectVisitor
    {
        private static readonly List<IVisitor> visitors;

        static ObjectVisitor()
        {
            visitors = new List<IVisitor>();

            // NOTE: The order here is intentional
            visitors.Add(new ExpandoObjectVisitor());
            visitors.Add(new DictionaryVisitor());
            visitors.Add(new ListVisitor());
            visitors.Add(new PocoVisitor());
        }

        public static IAdapter GetAdapter(OperationContext context)
        {
            while (context.MoveToNextPathSegment())
            {
                for (var i = 0; i < visitors.Count; i++)
                {
                    IAdapter adapter;
                    if ((adapter = visitors[i].GetAdapter(context)) != null)
                    {
                        return adapter;
                    }
                }
            }
            return null;
        }
    }
}
