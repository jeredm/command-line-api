// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class EntryPointDiscoverer
    {
        private class KeyComparer : IEqualityComparer<(Type, string)>
        {
            public static KeyComparer Default { get; } = new KeyComparer();

            public bool Equals((Type, string) x, (Type, string) y)
                => string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase) &&
                x.Item1 == y.Item1;

            public int GetHashCode((Type, string) obj) => (obj.Item2?.ToLowerInvariant().GetHashCode()) ?? 0 ^ obj.Item1.GetHashCode();
        }

        public static IReadOnlyCollection<MethodInfo> FindCommandMethods(Type containerType)
        {
            var foundMethods = new Dictionary<(Type, string), MethodInfo>(KeyComparer.Default);
            FindCommandMethods(containerType);
            return foundMethods.Values;

            bool FindCommandMethods(Type type)
            {
                Type[] nestedTypes = type.GetNestedTypes();

                bool nestedTypeAddedMethod = false;
                foreach (Type nestedType in nestedTypes)
                {
                    if (FindCommandMethods(nestedType))
                    {
                        nestedTypeAddedMethod = true;
                    }
                }

                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                                    BindingFlags.NonPublic).Where(m =>
                    m.ReturnType == typeof(void)
                    || m.ReturnType == typeof(int)
                    || m.ReturnType == typeof(Task)
                    || m.ReturnType == typeof(Task<int>)).ToArray();

                if (nestedTypeAddedMethod && methodInfos.Any()) throw new InvalidProgramException();

                bool addedMethod = false;
                foreach (MethodInfo method in methodInfos)
                {

                    if (foundMethods.ContainsKey((type, method.Name))) throw new AmbiguousMatchException($"Multiple methods named \"{method.Name}\" found.");
                    foundMethods.Add((type, method.Name), method);
                    addedMethod = true;

                }

                return addedMethod || nestedTypeAddedMethod;
            }


        }

        public static MethodInfo FindStaticEntryMethod(Assembly assembly)
        {
            var candidates = new List<MethodInfo>();
            foreach (TypeInfo type in assembly.DefinedTypes.Where(t =>
                !t.IsAbstract && string.Equals("Program", t.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                {
                    continue;
                }

                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                              BindingFlags.NonPublic).Where(m =>
                    string.Equals("Main", m.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (method.ReturnType == typeof(void)
                        || method.ReturnType == typeof(int)
                        || method.ReturnType == typeof(Task)
                        || method.ReturnType == typeof(Task<int>))
                    {
                        candidates.Add(method);
                    }
                }
            }

            if (candidates.Count > 1)
            {
                throw new AmbiguousMatchException(
                    "Ambiguous entry point. Found muliple static functions named 'Program.Main'. Could not identify which method is the main entry point for this function.");
            }

            if (candidates.Count == 0)
            {
                throw new InvalidProgramException(
                    "Could not find a static entry point named 'Main' on a type named 'Program' that accepts option parameters.");
            }

            MethodInfo entryMethod = candidates[0];
            return entryMethod;
        }
    }
}
