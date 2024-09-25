namespace Perkify.Core
{
    public enum ChainType
    {
        Any,
        All,
    }

    public class Chain : IEligible
    {
        private readonly ChainType chainType;

        private IList<Entitlement> entitlements = new List<Entitlement>();

        public Chain(ChainType chainType)
        {
            this.chainType = chainType;
        }

        #region Implement IEligible interface

        public bool IsEligible => this.chainType switch
        {
            ChainType.All => entitlements.All(entitlement => entitlement.IsEligible),
            ChainType.Any => entitlements.Any(entitlement => entitlement.IsEligible),
            _ => throw new InvalidOperationException(),
        };

        #endregion
    }
}
