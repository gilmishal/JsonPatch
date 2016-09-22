// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ExceptionHelper
    {
        public static void ThrowException(object targetObject, Operation operation, string message)
        {
            throw new JsonPatchException(new JsonPatchError(targetObject, operation, message));
        }
    }
}
