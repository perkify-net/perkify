namespace Perkify.Test.Sdk
{
    using System;
    using System.Collections.Generic;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.TestPriorityTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public TestPriorityAttribute() { }

        public TestPriorityAttribute(int priority) => this.Priority = priority;
    }

    public class TestPriorityTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var priority = traitAttribute.GetNamedArgument<int>("Priority");
            yield return new KeyValuePair<string, string>("Priority", priority.ToString());
        }
    }

    public class TestPriorityTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<XunitTestCase> OrderTestCases<XunitTestCase>(IEnumerable<XunitTestCase> testCases) where XunitTestCase : ITestCase
        {
            var traitType = typeof(TestPriorityAttribute).AssemblyQualifiedName!;
            return testCases.OrderBy(tc =>
            {
                int priority = tc.TestMethod.Method
                    .GetCustomAttributes(traitType)
                    .FirstOrDefault()
                    ?.GetNamedArgument<int>(nameof(TestPriorityAttribute.Priority))
                    ?? 0;
                var method = tc.TestMethod.Method.Name;
                return (priority, method);
            },
            Comparer<(int, string)>.Create((left, right) =>
            {
                var result = left.Item1.CompareTo(right.Item1);
                if (result == 0)
                {
                    result = left.Item2.CompareTo(right.Item2);
                }
                return result;
            }));
        }
    }
}
