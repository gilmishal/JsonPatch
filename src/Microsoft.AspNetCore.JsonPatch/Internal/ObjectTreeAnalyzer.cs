// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.JsonPatch.Exceptions;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    internal static class ObjectTreeAnalyzer
    {
        public static IPatchOperation Analyze(
            object objectToSearch,
            string path,
            IContractResolver contractResolver,
            Action<JsonPatchError> logErrorAction,
            Operation operation)
        {
            var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (objectToSearch == null)
                {
                    return new InvalidResult();
                }

                var currentPathSegment = pathSegments[i];

                // Exmaples:
                // - IDictionary<string, int>
                // - IDictionary<string, Customer>
                // - IDictionary<Guid, Customer>
                var dictType = ResultHelper.IsDictionary(objectToSearch);
                if (dictType != null)
                {
                    var keyType = dictType.GetTypeInfo().GenericTypeArguments[0];
                    var valueType = dictType.GetTypeInfo().GenericTypeArguments[1];
                    var concreteType = typeof(DictionaryWrapper<,>).MakeGenericType(keyType, valueType);
                    var dictionaryWrapper = (IDictionaryWrapper)Activator.CreateInstance(concreteType, args: objectToSearch);

                    // Do the ContainsKey check here to see if we want to dig further into the value returned.
                    // For example, the value returned could be a 'Customer' in which case the object to be searched
                    // becomes a 'Customer' and probing would continue as usual.
                    if (dictionaryWrapper.ContainsKey(currentPathSegment))
                    {
                        // Example: /USStatesProperty/WA
                        if (IsFinalSegment(pathSegments, i))
                        {
                            return new DictionaryPatchOperation(
                                dictionaryWrapper,
                                currentPathSegment,
                                logErrorAction,
                                path,
                                operation);
                        }
                        else
                        {
                            // Example: "/Customers/101/Address/Zipcode"
                            // Customers is a dictionary, 101 the key, Address the value which could be a POCO type 
                            // and Zipcode a property on it.
                            objectToSearch = dictionaryWrapper.GetValue(currentPathSegment);
                        }
                    }
                    else
                    {
                        // Example: "/Customers/101"
                        // 101 is the final segment here which we might be trying to add to the dictionary
                        if (IsFinalSegment(pathSegments, i))
                        {
                            return new DictionaryPatchOperation(
                                dictionaryWrapper,
                                currentPathSegment,
                                logErrorAction,
                                path,
                                operation);
                        }
                        else
                        {
                            // Example: 
                            // "/Customers/101/Address/Zipcode" is a valid path as "Address" exists and any operation 
                            // after that segment is potentially valid.
                            // "/Customers/101/Foo/Bar" is not a valid path as "Foo" does not exist and any segment
                            // coming after it cannot be valid as there is no value to probe into.
                            LogError(objectToSearch, operation, Resources.FormatCannotPerformOperation(operation.op, path), logErrorAction);
                        }
                    }//key not present in dictionary
                } // is a dictionary
                else
                {
                    var jsonContract = contractResolver.ResolveContract(objectToSearch.GetType());

                    if (jsonContract is JsonArrayContract)
                    {
                        // Example paths: "/Countries/0", "/Countries/-"
                        if (IsFinalSegment(pathSegments, i))
                        {
                            return new ArrayPatchOperation(objectToSearch, currentPathSegment, logErrorAction, path, operation);
                        }
                        else
                        {
                            // Example paths: "/Countries/0/States/0", "/Countries/0/States/-"
                            objectToSearch = ResultHelper.GetElementAtFromObject(objectToSearch, Convert.ToInt32(currentPathSegment));
                            if (objectToSearch == null)
                            {
                                LogError(objectToSearch, operation, Resources.FormatInvalidIndexForArrayProperty(operation.op, path), logErrorAction);
                                return new InvalidResult();
                            }
                        }
                    }
                    else if (jsonContract is JsonObjectContract)
                    {
                        var pocoProperty = ((JsonObjectContract)jsonContract)
                            .Properties
                            .FirstOrDefault(p => string.Equals(p.PropertyName, currentPathSegment, StringComparison.OrdinalIgnoreCase));

                        if (pocoProperty != null)
                        {
                            if (IsFinalSegment(pathSegments, i))
                            {
                                return new PocoPatchOperation(
                                    objectToSearch,
                                    currentPathSegment,
                                    pocoProperty,
                                    logErrorAction,
                                    path,
                                    operation);
                            }
                            else
                            {
                                objectToSearch = pocoProperty.ValueProvider.GetValue(objectToSearch);
                            }
                        }
                        else
                        {
                            LogError(
                                objectToSearch,
                                operation,
                                Resources.FormatCannotPerformOperation(operation.op, path),
                                logErrorAction);
                            return new InvalidResult();
                        }
                    }// jsonobject contract
                    else
                    {
                        throw new InvalidOperationException($"Uncrecognized contract:{jsonContract.GetType().FullName}");
                    }
                }// not a dictionary
            }

            return new InvalidResult();
        }

        private static bool IsFinalSegment(string[] pathSegments, int i)
        {
            return i == pathSegments.Length - 1;
        }

        private static void LogError(object targetObject, Operation operation, string message, Action<JsonPatchError> logErrorAction)
        {
            var jsonPatchError = new JsonPatchError(targetObject, operation, message);

            if (logErrorAction != null)
            {
                logErrorAction(jsonPatchError);
            }
            else
            {
                throw new JsonPatchException(jsonPatchError);
            }
        }
    }
}
