// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNetCore.JsonPatch.Test
{
    public class TestErrorLogger<T> where T : class
    {
        private readonly List<string> _errorMessages;

        public TestErrorLogger()
        {
            _errorMessages = new List<string>();
            ErrorMessages = _errorMessages.AsReadOnly();
        }

        public IReadOnlyList<string> ErrorMessages { get; }

        public void LogErrorMessage(JsonPatchError patchError)
        {
            _errorMessages.Add(patchError.ErrorMessage);
        }
    }
}
