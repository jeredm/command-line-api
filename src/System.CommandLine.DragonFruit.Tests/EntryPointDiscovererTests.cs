// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class EntryPointDiscovererTests
    {
        [Fact]
        public void ItThrowsIfEntryPointNotFound()
        {
            Action find = () => EntryPointDiscoverer.FindStaticEntryMethod(typeof(IEnumerable<>).Assembly);
            find.Should().Throw<InvalidProgramException>();
        }

        [Fact]
        public void ItThrowsIfMultipleEntryPointNotFound()
        {
            Action find = () => EntryPointDiscoverer.FindStaticEntryMethod(typeof(EntryPointDiscovererTests).Assembly);
            find.Should().Throw<AmbiguousMatchException>();
        }

        [Fact]
        public void ItFindAllPublicStaticMethods()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(Program));
            methods.Should().HaveCount(2);
        }

        [Fact]
        public void IfTwoMethodsHaveTheSameNameItThorsAmbigiousMatchException()
        {
            Action find = () => EntryPointDiscoverer.FindCommandMethods(typeof(IHaveDuplicateMethodNames));
            find.Should().Throw<AmbiguousMatchException>();
        }

        [Fact]
        public void ItFindMethodsInNestedClasses()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveNestedClass));
            methods.Should().HaveCount(2);
        }

        [Fact]
        public void ItFindsMethodsInNestedStaticClass()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveNestedStaticClass));
            methods.Should().HaveCount(2);
        }

        [Fact]
        public void IfTypeContainsNestedMethodsItCannotHaveParentMethod()
        {
            Action find = () => EntryPointDiscoverer.FindCommandMethods(typeof(IHaveNestedClassAndInvalidRootMethod));
            find.Should().Throw<InvalidProgramException>();
        }

        [Fact]
        public void ItDoesNotFindInstanceMethods()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveInstanceMethods));
            methods.Should().HaveCount(0);
        }
        
        [Fact]
        public void ItOnlyFindPublicAndInternalMethods()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveInstanceMethods));
            methods.Should().HaveCount(3);
        }

        [Fact]
        public void ItAllowsDuplicatedMethodNamesInDifferentNestedTypes()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveMultipleNestedClasses));
            methods.Should().HaveCount(2);
        }

        [Fact]
        public void ItAllowsNestedClassWithoutCommandMethodsWithRootLevelMethods()
        {
            var methods = EntryPointDiscoverer.FindCommandMethods(typeof(IHaveNestedClassWithoutValidMethods));
            methods.Should().HaveCount(1);
        }

        private class Program
        {
            public static void Main(string arg1) { }
            public static void Main2(string arg2, string arg3) { }
        }

        private class IHaveDuplicateMethodNames
        {
            public static void Main(string arg1) { }
            internal static void main(string arg1, string arg2) { }
        }

        private class IHaveNestedClass
        {
            public class Add
            {
                public static void Reference(string name) { }
                public static void Package(string name) { }
            }
        }

        public class IHaveMultipleNestedClasses
        {
            public class Add
            {
                public static void Reference() { }
            }

            public class Remove
            {
                public static void Reference() { }
            }
        }

        private class IHaveNestedStaticClass
        {
            public static class Add
            {
                public static void Reference(string name) { }
                public static void Package(string name) { }
            }
        }

        private class IHaveNestedClassAndInvalidRootMethod
        {
            internal static void InvalidMethod() { }

            public class Add
            {
                public static void Reference(string name) { }
                public static void Package(string name) { }
            }
        }

        private class IHaveNestedClassWithoutValidMethods
        {
            public static void Main() { }

            public class InnerClass
            {
                public void DoSomething() { }

                private static void NotACommandMethod() { }
            }
        }

        private class IHaveInstanceMethods
        {
            public void PublicInstance() { }
            internal void InternalInstance() { }
        }

        public class IHaveAllProtectedLevels
        {
            public static void Public() { }
            internal static void Internal() { }
            protected internal static void ProtectedInternal() { }
            protected private static void ProtectedPrivate() { }
            private static void Private() { }
        }

    }
}
