// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.JsonPatch.Internal
{
    public static class ObjectTreeAnalyzer
    {
        public static IPatchObject Analyze(
            object targetObject,
            string path,
            IContractResolver contractResolver,
            Operation operation)
        {
            IPatchObject patchOperation = null;

            var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (targetObject == null)
                {
                    break;
                }

                var currentPathSegment = pathSegments[i];
                ExpandoObject expandoObject = null;
                IDictionary dictionary = null;
                IList list = null;
                if ((dictionary = targetObject as IDictionary) != null)
                {
                    // Example: "/USStates/WA"
                    if (IsFinalSegment(pathSegments, i))
                    {
                        patchOperation = new PatchDictionaryObject(dictionary, currentPathSegment, operation);
                        break;
                    }
                    else if (dictionary.Contains(currentPathSegment))
                    {
                        // Example path: "/Customers/101/Address/Zipcode" and
                        // let's say the current path segment is "101"
                        targetObject = dictionary[currentPathSegment];
                    }
                    else
                    {
                        break;
                    }
                }
                else if ((expandoObject = targetObject as ExpandoObject) != null)
                {
                    // Example: /USStatesProperty/WA
                    if (IsFinalSegment(pathSegments, i))
                    {
                        patchOperation = new PatchExpandoObject(expandoObject, currentPathSegment, operation);
                        break;
                    }
                    else if (expandoObject.ContainsCaseInsensitiveKey(currentPathSegment))
                    {
                        // Example path: "/Customers/101/Address/Zipcode" and
                        // let's say the current path segment is "101"
                        targetObject = expandoObject.GetValueForCaseInsensitiveKey(currentPathSegment);
                    }
                    else
                    {
                        break;
                    }
                }
                else if ((list = targetObject as IList) != null)
                {
                    // Example paths: "/Countries/0", "/Countries/-"
                    if (IsFinalSegment(pathSegments, i))
                    {
                        patchOperation = new PatchListObject(list, currentPathSegment, operation);
                        break;
                    }
                    else
                    {
                        // Example paths: "/Countries/0/States/0", "/Countries/0/States/-"
                        targetObject = GetElementAtFromObject(targetObject, Convert.ToInt32(currentPathSegment));
                        if (targetObject == null)
                        {
                            throw new JsonPatchException(new JsonPatchError(targetObject, operation, Resources.FormatInvalidIndexForArrayProperty(operation.op, path)));
                        }
                    }
                }
                else
                {
                    var jsonContract = contractResolver.ResolveContract(targetObject.GetType());
                    JsonObjectContract jsonObjectContract = null;
                    if ((jsonObjectContract = jsonContract as JsonObjectContract) != null)
                    {
                        var pocoProperty = jsonObjectContract
                            .Properties
                            .FirstOrDefault(p => string.Equals(p.PropertyName, currentPathSegment, StringComparison.OrdinalIgnoreCase));

                        if (pocoProperty != null)
                        {
                            if (IsFinalSegment(pathSegments, i))
                            {
                                patchOperation = new PatchPocoObject(targetObject, pocoProperty, operation);
                                break;
                            }
                            else
                            {
                                targetObject = pocoProperty.ValueProvider.GetValue(targetObject);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }// jsonobject contract
                    else
                    {
                        throw new InvalidOperationException($"Uncrecognized contract:{jsonContract.GetType().FullName}");
                    }
                }
            }// foreach path segment

            if (patchOperation == null)
            {
                throw new JsonPatchException(new JsonPatchError(targetObject, operation, Resources.FormatCannotPerformOperation(operation.op, path)));
            }

            return patchOperation;
        }

        private static bool IsFinalSegment(string[] pathSegments, int i)
        {
            return i == pathSegments.Length - 1;
        }

        private static object GetElementAtFromObject(object targetObject, int index)
        {
            var list = targetObject as IList;
            if (list != null && index >= 0 && index < list.Count)
            {
                return list[index];
            }
            return null;
        }
    }
}
