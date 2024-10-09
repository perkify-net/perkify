namespace Perkify.Test.Sdk
{
    using System;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.UnitTestTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnitTestAttribute : Attribute
    {
        public string Tier { get; private set; } = string.Empty;

        public UnitTestAttribute() { }

        public UnitTestAttribute(string tier) => this.Tier = tier;
    }

    public class UnitTestTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "UnitTest");

            var name = traitAttribute.GetNamedArgument<string>("Tier");
            if (!string.IsNullOrWhiteSpace(name))
                yield return new KeyValuePair<string, string>("UnitTestTier", name);
        }
    }
}
