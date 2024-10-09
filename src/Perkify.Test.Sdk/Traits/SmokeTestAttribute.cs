namespace Perkify.Test.Sdk
{
    using System;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.SmokeTestTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SmokeTestAttribute : Attribute, ITraitAttribute
    {
        public SmokeTestAttribute() { }
    }

    public class SmokeTestTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("SmokeTest", "true");
        }
    }
}
