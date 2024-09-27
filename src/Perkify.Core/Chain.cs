namespace Perkify.Core
{
    public class Chain : IEligible
    {
        private IList<Entitlement> entitlements = new List<Entitlement>();

        #region Implement IEligible interface

        public bool IsEligible => entitlements.Any(entitlement => entitlement.IsEligible);

        #endregion
    }
}
