// <copyright file="Entitlement.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IBalance<Entitlement>
    {
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

        /// <inheritdoc/>
        public Entitlement Clear()
        {
            this.balance.Clear();
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.expiry.Renew();
            }

            return this;
        }
    }
}
