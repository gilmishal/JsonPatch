// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class OperationContext
    {
        private int _index;
        private readonly string _path;
        private readonly string[] _pathSegments;

        public OperationContext(
            string path,
            object targetObject,
            Operation operation,
            IContractResolver contractResolver)
        {
            _path = path;
            _pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            _index = -1;

            TargetObject = targetObject;
            Operation = operation;
            ContractResolver = contractResolver;
        }

        public IContractResolver ContractResolver { get; }
        public Operation Operation { get; }
        public object TargetObject { get; private set; }

        public bool TryGetSegment(out PathSegment pathSegment)
        {
            if (_index + 1 < _pathSegments.Length)
            {
                _index++;

                var isFinalSegment = _index == _pathSegments.Length - 1;
                pathSegment = new PathSegment(_pathSegments[_index], isFinalSegment);
                return true;
            }
            pathSegment = default(PathSegment);
            return false;
        }

        public void SetNewTargetObject(object newTargetObject)
        {
            TargetObject = newTargetObject;
        }
    }
}
