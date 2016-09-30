// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Xunit;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public class DictionaryAdapterTest
    {
        [Fact]
        public void Add_KeyWhichAlreadyExists_ReplacesExistingValue()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            dictionary[nameKey] = "Mike";
            var operation = new Operation("add", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Add("James");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("James", dictionary[nameKey]);

            // Arrange
            patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Add("Michael");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("Michael", dictionary[nameKey]);
        }

        [Fact]
        public void Add_AddsValue_UsingCaseSensitiveKey_FailureScenario()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            var operation = new Operation("add", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Add("James");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("James", dictionary[nameKey]);

            // Act
            patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey.ToUpper(), operation);
            var value = patchDictionaryObject.Get();
            Assert.Null(value);
        }

        [Fact]
        public void Add_AddsValue_UsingCaseSensitiveKey_SuccessScenario()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            var operation = new Operation("add", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Add("James");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("James", dictionary[nameKey]);

            // Act
            patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);
            var value = patchDictionaryObject.Get();
            Assert.Equal(value, "James");
        }

        [Fact]
        public void Add_AddsValue_CaseInsensitively_ForCaseInsensitiveDictionaries()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var operation = new Operation("add", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Add("James");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("James", dictionary[nameKey]);

            // Act
            patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey.ToUpper(), operation);
            var value = patchDictionaryObject.Get();
            Assert.Equal(value, "James");
        }

        [Fact]
        public void ReplacingExistingItem()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            dictionary.Add(nameKey, "Mike");
            var operation = new Operation("replace", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Replace("James");

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("James", dictionary[nameKey]);
        }

        [Fact]
        public void Replace_NonExistingKey_FailsWithException()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            var operation = new Operation("replace", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                patchDictionaryObject.Replace("Mike");
            });

            Assert.Equal(
                $"For operation '{operation.op}', the target location specified by path '{operation.path}' was not found.",
                exception.Message);
        }

        [Fact]
        public void Remove_NonExistingKey_FailsWithException()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            var operation = new Operation("remove", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act & Assert
            var exception = Assert.Throws<JsonPatchException>(() =>
            {
                patchDictionaryObject.Remove();
            });

            Assert.Equal(
                $"For operation '{operation.op}', the target location specified by path '{operation.path}' was not found.",
                exception.Message);
        }

        [Fact]
        public void Remove_RemovesFromDictionary()
        {
            // Arrange
            var nameKey = "Name";
            var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            dictionary[nameKey] = "James";
            var operation = new Operation("remove", $"/{nameKey}", from: null);
            var patchDictionaryObject = new DictionaryAdapter(dictionary, nameKey, operation);

            // Act
            patchDictionaryObject.Remove();

            //Assert
            Assert.Equal(0, dictionary.Count);
        }
    }
}
