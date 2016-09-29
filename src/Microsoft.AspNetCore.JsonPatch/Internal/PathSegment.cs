// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public struct PathSegment
    {
        public PathSegment(string value, bool isFinal)
        {
            Value = value;
            IsFinal = isFinal;
        }

        public string Value { get; }

        public bool IsFinal { get; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(PathSegment pathSegment)
        {
            return pathSegment.ToString();
        }
    }
}
