// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Xunit;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class ListAdapterTest
    {
        [Fact]
        public void Patch_OnArrayObject_FailsWithException()
        {
            // Arrange
            var targetObject = new[] { 20, 30 };

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                new ListAdapter(targetObject, "0", new Operation("add", "/numbers", from: null));
            });

            Assert.Equal(
                $"The type '{targetObject.GetType().FullName}' which is an array is not supported for json " +
                "patch operations as it has a fixed size.",
                exception.Message);
        }

        [Fact]
        public void Patch_OnNonGenericListObject_FailsWithException()
        {
            // Arrange
            var targetObject = new ArrayList();
            targetObject.Add(20);
            targetObject.Add(30);

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                new ListAdapter(targetObject, "0", new Operation("add", "/numbers", from: null));
            });

            Assert.Equal(
                $"The type '{targetObject.GetType().FullName}' which is a non generic list is not supported for json " +
                "patch operations. Only generic list types are supported.",
                exception.Message);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("-2")]
        [InlineData("2")]
        [InlineData("3")]
        public void Patch_WithOutOfBoundsIndex_FailsWithException(string position)
        {
            // Arrange
            var targetObject = new List<string>() { "James", "Mike" };
            var operation = new Operation("add", "/Names", from: null);

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                new ListAdapter(targetObject, position, operation);
            });

            Assert.Equal(
                $"For operation '{operation.op}' on array property at path '{operation.path}', the index is out of " +
                "bounds of the array size.",
                exception.Message);
        }

        [Theory]
        [InlineData("_")]
        [InlineData("blah")]
        public void Patch_WithInvalidPositionFormat_FailsWithException(string position)
        {
            // Arrange
            var targetObject = new List<string>() { "James", "Mike" };
            var operation = new Operation("add", "/Names", from: null);

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                new ListAdapter(targetObject, position, operation);
            });

            Assert.Equal(
                $"For operation '{operation.op}', the provided path is invalid for array property at path '{operation.path}'.",
                exception.Message);
        }

        public static TheoryData<List<int>, List<int>> AppendAtEndOfListData
        {
            get
            {
                return new TheoryData<List<int>, List<int>>()
                {
                    {
                        new List<int>() {  },
                        new List<int>() { 20 }
                    },
                    {
                        new List<int>() { 5, 10 },
                        new List<int>() { 5, 10, 20 }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(AppendAtEndOfListData))]
        public void Add_Appends_AtTheEnd(List<int> targetObject, List<int> expected)
        {
            // Arrange
            var pathListObject = GetpathListObject(targetObject, "-");

            // Act
            pathListObject.Add(20);

            // Assert
            Assert.Equal(expected.Count, targetObject.Count);
            Assert.Equal(expected, targetObject);
        }

        [Fact]
        public void Add_NullObject_ToReferenceTypeListWorks()
        {
            // Arrange
            var targetObject = new List<string>() { "James", "Mike" };
            var pathListObject = GetpathListObject(targetObject, "-");

            // Act
            pathListObject.Add(value: null);

            // Assert
            Assert.Equal(3, targetObject.Count);
            Assert.Equal(new List<string>() { "James", "Mike", null }, targetObject);
        }

        [Fact]
        public void Add_NonCompatibleType_FailsWithException()
        {
            // Arrange
            var targetObject = new List<int>() { 10, 20 };
            var operation = new Operation("add", "/Numbers/-", from: null);
            var pathListObject = GetpathListObject(targetObject, "-", operation);

            // Act & Assert

            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                pathListObject.Add("Mike");
            });

            Assert.Equal($"The value 'Mike' is invalid for property at path '{operation.path}'.", exception.Message);
        }

        public static TheoryData<IList, object, string, IList> AddingDifferentComplexTypeWorksData
        {
            get
            {
                return new TheoryData<IList, object, string, IList>()
                {
                    {
                        new List<string>() { },
                        "a",
                        "-",
                        new List<string>() { "a" }
                    },
                    {
                        new List<string>() { "a", "b" },
                        "c",
                        "-",
                        new List<string>() { "a", "b", "c" }
                    },
                    {
                        new List<string>() { "a", "b" },
                        "c",
                        "0",
                        new List<string>() { "c", "a", "b" }
                    },
                    {
                        new List<string>() { "a", "b" },
                        "c",
                        "1",
                        new List<string>() { "a", "c", "b" }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddingDifferentComplexTypeWorksData))]
        public void Add_DifferentComplexTypeWorks(IList targetObject, object value, string position, IList expected)
        {
            // Arrange
            var pathListObject = GetpathListObject(targetObject, position);

            // Act
            pathListObject.Add(value);

            // Assert
            Assert.Equal(expected.Count, targetObject.Count);
            Assert.Equal(expected, targetObject);
        }

        [Fact]
        public void Replace_NonCompatibleType_FailsWithException()
        {
            // Arrange
            var targetObject = new List<int>() { 10, 20 };
            var operation = new Operation("add", "/Numbers/-", from: null);
            var pathListObject = GetpathListObject(targetObject, "-", operation);

            // Act & Assert

            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                pathListObject.Replace("Mike");
            });

            Assert.Equal($"The value 'Mike' is invalid for property at path '{operation.path}'.", exception.Message);
        }

        [Fact]
        public void Replace_ReplacesValue_AtTheEnd()
        {
            // Arrange
            var targetObject = new List<int>() { 10, 20 };
            var operation = new Operation("add", "/Numbers/-", from: null);
            var pathListObject = GetpathListObject(targetObject, "-", operation);

            // Act
            pathListObject.Replace(30);

            // Assert
            Assert.Equal(new List<int>() { 10, 30 }, targetObject);
        }

        public static TheoryData<string, List<int>> ReplacesValuesAtPositionData
        {
            get
            {
                return new TheoryData<string, List<int>>()
                {
                    {
                        "0",
                        new List<int>() { 30, 20 }
                    },
                    {
                        "1",
                        new List<int>() { 10, 30 }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ReplacesValuesAtPositionData))]
        public void Replace_ReplacesValue_AtGivenPosition(string position, List<int> expected)
        {
            // Arrange
            var targetObject = new List<int>() { 10, 20 };
            var operation = new Operation("add", "/Numbers/-", from: null);
            var pathListObject = GetpathListObject(targetObject, position, operation);

            // Act
            pathListObject.Replace(30);

            // Assert
            Assert.Equal(expected, targetObject);
        }

        private ListAdapter GetpathListObject(
            IList targetObject,
            string pathSegment,
            Operation operation = null)
        {
            if (operation == null)
            {
                operation = new Operation("add", "/numbers", from: null);
            }
            return new ListAdapter(targetObject, pathSegment, operation);
        }
    }
}
