namespace Perkify.Test.Sdk
{
    using System;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.FlakyTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class FlakyAttribute : Attribute, ITraitAttribute
    {
        public FlakyAttribute() { }
    }

    public class FlakyTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Flaky", "true");
        }
    }
}
