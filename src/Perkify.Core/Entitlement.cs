﻿namespace Perkify.Core
{
    [Flags]
    public enum AutoRenewalMode
    {
        None = 0x0000,
        Topup = 0x0001,
        Deduct = 0x0002,
        Adjust = 0x0004,
        Default = Topup | Deduct
    }

    public class Entitlement : IEligible, IBalance<Entitlement>, INowUtc, IExpiry<Entitlement>
    {
        private readonly AutoRenewalMode mode;

        private Expiry? expiry;

        private Balance? balance;

        private IEligible? prerequesite;

        public Entitlement(AutoRenewalMode mode = AutoRenewalMode.Default)
        {
            this.mode = mode;
        }

        public Entitlement WithBalance(Balance balance)
        {
            this.balance = balance;
            return this;
        }

        public Entitlement WithExpiry(Expiry expiry)
        {
            this.expiry = expiry;
            return this;
        }

        public Entitlement WithPrerequesite(IEligible prerequesite)
        {
            this.prerequesite = prerequesite;
            return this;
        }

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
            if(this.mode.HasFlag(AutoRenewalMode.Topup))
            {
                this.expiry?.Renew();
            }
            return this;
        }

        public Entitlement Deduct(long delta, bool overspending = true)
        {
            this.balance?.Deduct(delta, overspending);
            if(this.mode.HasFlag(AutoRenewalMode.Deduct))
            {
                this.expiry?.Renew();
            }
            return this;
        }

        public Entitlement Adjust(long? incoming, long? outgoing)
        {
            this.balance?.Adjust(incoming, outgoing);
            if (this.mode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.expiry?.Renew();
            }
            return this;
        }

        #endregion

        #region Implements INowUtc interface

        public DateTime NowUtc => this.expiry?.NowUtc ?? DateTime.UtcNow;

        #endregion

        #region Implements IExpiry<T> interface

        #region Remaining & Overdue

        /// <summary>The Grace period as absolute time span.</summary>
        public TimeSpan GracePeriod => this.expiry?.GracePeriod ?? TimeSpan.Zero;

        /// <summary>
        /// The remaining portion.
        /// - If suspended, the remaining portion is the time between suspend time and expiry time.
        /// - If eligible, the remaining portion is the current time between now and expiry time.
        /// - If ineligible (after grace period), the remaining portion is negative grace period.
        /// </summary>
        public TimeSpan Remaining => this.expiry?.Remaining ?? TimeSpan.MaxValue;

        /// <summary>
        /// The overdue portion.
        /// - Suspended: The overdue portion is the time between suspend time and expiry time. Zero if suspend time is earlier than expiry time.
        /// - Eligible: The overdue portion is the time between expiry time and now. Zero if now is earlier than expiry time.
        /// - Ineligible (after grace period): The overdue portion is the grace period.
        /// </summary>
        public TimeSpan Overdue => this.expiry?.Overdue ?? TimeSpan.Zero;

        #endregion

        #region Renew

        /// <summary>The expiry time in UTC.</summary>
        public DateTime ExpiryUtc => this.expiry?.ExpiryUtc ?? DateTime.MaxValue;

        /// <summary>The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
        public Renewal? Renewal => this.expiry?.Renewal;

        /// <summary>Renew the expiry time in timeline arithmetic or calendrical arithmetic.</summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        /// <returns>The expiry time after renewal.</returns>
        public Entitlement Renew(Renewal? renewal)
        {
            this.expiry?.Renew(renewal);
            return this;
        }

        #endregion

        #region Deactivate & Activate

        /// <summary>
        /// The suspend time in UTC.
        /// - Explicit suspension time (smaller or equals to deadline time) if any suspension is applied.
        /// - Implicit suspension time (equals to deadline time) when the expiry time is ineligible.
        /// - Null when the expiry time is eligible (and no suspension is applied).
        /// </summary>
        public DateTime? SuspensionUtc => this.expiry?.SuspensionUtc;

        /// <summary>
        /// Boolean flag to identify if the expiry time is active.
        /// - True if the expiry time is deactivated (suspended).
        /// - False if the expiry time is activated (not suspended).
        /// </summary>
        public bool IsActive => this.expiry?.IsActive ?? true;

        /// <summary>Deactivate the expiry time.</summary>
        /// <param name="suspendUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier than current time.</exception>
        public Entitlement Deactivate(DateTime? suspensionUtc = null)
        {
            this.expiry?.Deactivate(suspensionUtc);
            return this;
        }

        /// <summary>Activate the expiry time.</summary>
        /// <param name="resumptionUtc">The resumption time in UTC.</param>
        /// <param name="extended">Boolean flag to extend the expiry time after resumption.</param>
        /// <returns>The expiry time after resumption.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Resume time must be greater than suspend time.</exception>
        public Entitlement Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            this.expiry?.Activate(resumptionUtc, extended);
            return this;
        }

        #endregion

        #endregion
    }
}
