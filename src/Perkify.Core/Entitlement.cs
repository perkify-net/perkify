// <copyright file="Entitlement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents an entitlement with balance and expiry management.
    /// </summary>
    public class Entitlement : IEligible, IBalance<Entitlement>, INowUtc, IExpiry<Entitlement>
    {
        private readonly Balance balance;
        private readonly Expiry? expiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entitlement"/> class with balance, expiry, and auto-renewal mode.
        /// </summary>
        /// <param name="balance">The balance associated with the entitlement.</param>
        /// <param name="expiry">The expiry associated with the entitlement.</param>
        /// <param name="mode">The auto-renewal mode.</param>
        public Entitlement(Balance balance, Expiry expiry, AutoRenewalMode mode = AutoRenewalMode.Default)
        {
            this.balance = balance;
            this.expiry = expiry;
            this.AutoRenewalMode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entitlement"/> class with balance and default auto-renewal mode.
        /// </summary>
        /// <param name="balance">The balance associated with the entitlement.</param>
        public Entitlement(Balance balance)
        {
            this.balance = balance;
            this.expiry = null;
            this.AutoRenewalMode = AutoRenewalMode.None;
        }

        /// <summary>
        /// Gets the auto-renewal mode.
        /// </summary>
        public AutoRenewalMode AutoRenewalMode { get; private set; }

        /// <summary>
        /// Gets the prerequisite eligibility.
        /// </summary>
        public IEligible? Prerequesite { get; init; }

        #region Implements IEligible interface

        /// <inheritdoc/>
        public bool IsEligible =>
            (this.balance?.IsEligible ?? true)
            && (this.expiry?.IsEligible ?? true)
            && (this.Prerequesite?.IsEligible ?? true);

        #endregion

        #region Implements IBalance<T> interface

        /// <inheritdoc/>
        public long Incoming => this.balance?.Incoming ?? 0;

        /// <inheritdoc/>
        public long Outgoing => this.balance?.Outgoing ?? 0;

        /// <inheritdoc/>
        public long Threshold => this.balance?.Threshold ?? 0;

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            this.balance.Topup(delta);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
            {
                this.expiry.Renew();
            }
        }

        /// <inheritdoc/>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            var result = this.balance.Deduct(delta, policy);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
            {
                this.expiry.Renew();
            }

            return result;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public DateTime NowUtc => this.expiry?.NowUtc ?? DateTime.UtcNow;

        #endregion

        #region Implements IExpiry<T> interface

        #region Remaining & Overdue

        /// <inheritdoc/>
        public TimeSpan GracePeriod => this.expiry?.GracePeriod ?? TimeSpan.Zero;

        /// <inheritdoc/>
        public TimeSpan Remaining => this.expiry?.Remaining ?? TimeSpan.MaxValue;

        /// <inheritdoc/>
        public TimeSpan Overdue => this.expiry?.Overdue ?? TimeSpan.Zero;

        #endregion

        #region Renew

        /// <inheritdoc/>
        public DateTime ExpiryUtc => this.expiry?.ExpiryUtc ?? DateTime.MaxValue;

        /// <inheritdoc/>
        public Renewal? Renewal => this.expiry?.Renewal;

        /// <inheritdoc/>
        public Entitlement Renew(Renewal? renewal)
        {
            this.expiry?.Renew(renewal);
            return this;
        }

        #endregion

        #region Deactivate & Activate

        /// <inheritdoc/>
        public DateTime? SuspensionUtc => this.expiry?.SuspensionUtc;

        /// <inheritdoc/>
        public bool IsActive => this.expiry?.IsActive ?? true;

        /// <inheritdoc/>
        public Entitlement Deactivate(DateTime? suspensionUtc = null)
        {
            this.expiry?.Deactivate(suspensionUtc);
            return this;
        }

        /// <inheritdoc/>
        public Entitlement Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            this.expiry?.Activate(resumptionUtc, extended);
            return this;
        }

        #endregion

        #endregion
    }
}
