// <copyright file="Entitlement.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IBalance
    {
        /// <inheritdoc/>
        public long Incoming
            => this.balance!.Incoming;

        /// <inheritdoc/>
        public long Outgoing
            => this.balance!.Outgoing;

        /// <inheritdoc/>
        public long Threshold
            => this.balance!.Threshold;

        /// <inheritdoc/>
        public BalanceType BalanceType
            => this.balance!.BalanceType;

        /// <inheritdoc/>
        public long Gross
            => this.balance!.Gross;

        /// <inheritdoc/>
        public long Overspending
            => this.balance!.Overspending;

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            this.balance!.Topup(delta);

            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
            {
                this.expiry?.Renew();
            }
        }

        /// <inheritdoc/>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            var result = this.balance!.Deduct(delta, policy);

            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
            {
                this.expiry?.Renew();
            }

            return result;
        }

        /// <inheritdoc/>
        public void Adjust(long? incoming, long? outgoing)
        {
            this.balance!.Adjust(incoming, outgoing);
            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.expiry?.Renew();
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.balance!.Clear();
            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.expiry?.Renew();
            }
        }
    }
}
