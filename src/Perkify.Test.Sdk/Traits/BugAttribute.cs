namespace Perkify.Test.Sdk
{
    using System;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [TraitDiscoverer($"{TraitDiscovererAssembly.Name}.BugTraitDiscoverer", TraitDiscovererAssembly.Name)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BugAttribute : Attribute
    {
        public string Id { get; private set; } = string.Empty;

        public BugAttribute() { }

        public BugAttribute(ulong id) : this(id.ToString()) { }

        public BugAttribute(string id) => this.Id = id;
    }

    public class BugTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "Bug");

            var id = traitAttribute.GetNamedArgument<string>("Id");
            if (!string.IsNullOrWhiteSpace(id))
                yield return new KeyValuePair<string, string>("Bug", id);
        }
    }
}
