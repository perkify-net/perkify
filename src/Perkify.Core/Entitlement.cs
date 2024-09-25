namespace Perkify.Core
{
    public class Entitlement : IEligible, IBalance<Entitlement>
    {
        private readonly Balance? balance;

        private readonly Expiry? expiry;

        private readonly IEligible? prerequesite;

        private readonly Renewal? renewal;

        #region Implements IEligible interface

        public bool IsEligible =>
            (this.balance?.IsEligible ?? true) 
            && (this.expiry?.IsEligible ?? true) 
            && (this.prerequesite?.IsEligible ?? true);

        #endregion

        #region Implements IBalance<T> interface

        public long Incoming => this.balance?.Incoming ?? 0;

        public long Outgoing => this.balance?.Outgoing ?? 0;

        public long Threshold => this.balance?.Threshold ?? 0;

        public Entitlement Topup(long delta)
        {
            this.balance?.Topup(delta);
            return this;
        }

        public Entitlement Spend(long delta, bool overspending = true)
        {
            this.balance?.Spend(delta, overspending);
            return this;
        }

        public Entitlement Adjust(long? incoming, long? outgoing)
        {
            this.balance?.Adjust(incoming, outgoing);
            return this;
        }

        #endregion
    }
}
