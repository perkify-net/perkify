namespace Perkify.Test.Sdk
{
    using System;
    using System.Collections.Generic;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.TestCategoryTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class TestCategoryAttribute : Attribute
    {
        public string Name { get; private set; } = string.Empty;

        public TestCategoryAttribute() { }

        public TestCategoryAttribute(string name) => this.Name = name;
    }

    public class TestCategoryTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var category = traitAttribute.GetNamedArgument<string>("Category");
            yield return new KeyValuePair<string, string>("Category", category);
        }
    }
}
