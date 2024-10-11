// <copyright file="Entitlement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    public class Entitlement : IEligible, IBalance<Entitlement>, INowUtc, IExpiry<Entitlement>
    {
        private readonly Balance balance;

        private readonly Expiry? expiry;

        public Entitlement(Balance balance, Expiry expiry, AutoRenewalMode mode = AutoRenewalMode.Default)
        {
            this.balance = balance;
            this.expiry = expiry;
            this.AutoRenewalMode = mode;
        }

        public Entitlement(Balance balance)
        {
            this.balance = balance;
            this.expiry = null;
            this.AutoRenewalMode = AutoRenewalMode.None;
        }

        public AutoRenewalMode AutoRenewalMode { get; private set; }

        public IEligible? Prerequesite { get; init; }

        #region Implements IEligible interface

        public bool IsEligible =>
            (this.balance?.IsEligible ?? true)
            && (this.expiry?.IsEligible ?? true)
            && (this.Prerequesite?.IsEligible ?? true);

        #endregion

        #region Implements IBalance<T> interface

        public long Incoming => this.balance?.Incoming ?? 0;

        public long Outgoing => this.balance?.Outgoing ?? 0;

        public long Threshold => this.balance?.Threshold ?? 0;

        public void Topup(long delta)
        {
            this.balance.Topup(delta);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
            {
                this.expiry.Renew();
            }
        }

        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            var result = this.balance.Deduct(delta, policy);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
            {
                this.expiry.Renew();
            }

            return result;
        }

        public Entitlement Adjust(long? incoming, long? outgoing)
        {
            this.balance.Adjust(incoming, outgoing);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.expiry.Renew();
            }

            return this;
        }

        #endregion

        #region Implements INowUtc interface

        public DateTime NowUtc => this.expiry?.NowUtc ?? DateTime.UtcNow;

        #endregion

        #region Implements IExpiry<T> interface

        #region Remaining & Overdue

        /// <summary>Gets the Grace period as absolute time span.</summary>
        public TimeSpan GracePeriod => this.expiry?.GracePeriod ?? TimeSpan.Zero;

        /// <summary>
        /// Gets the remaining portion.
        /// - If suspended, the remaining portion is the time between suspend time and expiry time.
        /// - If eligible, the remaining portion is the current time between now and expiry time.
        /// - If ineligible (after grace period), the remaining portion is negative grace period.
        /// </summary>
        public TimeSpan Remaining => this.expiry?.Remaining ?? TimeSpan.MaxValue;

        /// <summary>
        /// Gets the overdue portion.
        /// - Suspended: The overdue portion is the time between suspend time and expiry time. Zero if suspend time is earlier than expiry time.
        /// - Eligible: The overdue portion is the time between expiry time and now. Zero if now is earlier than expiry time.
        /// - Ineligible (after grace period): The overdue portion is the grace period.
        /// </summary>
        public TimeSpan Overdue => this.expiry?.Overdue ?? TimeSpan.Zero;

        #endregion

        #region Renew

        /// <summary>Gets the expiry time in UTC.</summary>
        public DateTime ExpiryUtc => this.expiry?.ExpiryUtc ?? DateTime.MaxValue;

        /// <summary>Gets the renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
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
        /// Gets the suspend time in UTC.
        /// - Explicit suspension time (smaller or equals to deadline time) if any suspension is applied.
        /// - Implicit suspension time (equals to deadline time) when the expiry time is ineligible.
        /// - Null when the expiry time is eligible (and no suspension is applied).
        /// </summary>
        public DateTime? SuspensionUtc => this.expiry?.SuspensionUtc;

        /// <summary>
        /// Gets a value indicating whether boolean flag to identify if the expiry time is active.
        /// - True if the expiry time is deactivated (suspended).
        /// - False if the expiry time is activated (not suspended).
        /// </summary>
        public bool IsActive => this.expiry?.IsActive ?? true;

        /// <summary>Deactivate the expiry time.</summary>
        /// <param name="suspensionUtc">The suspension time in UTC.</param>
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
