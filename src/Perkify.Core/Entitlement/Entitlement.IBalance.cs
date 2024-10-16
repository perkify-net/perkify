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
            => this.Balance!.Incoming;

        /// <inheritdoc/>
        public long Outgoing
            => this.Balance!.Outgoing;

        /// <inheritdoc/>
        public long Threshold
            => this.Balance!.Threshold;

        /// <inheritdoc/>
        public BalanceType BalanceType
            => this.Balance!.BalanceType;

        /// <inheritdoc/>
        public long Gross
            => this.Balance!.Gross;

        /// <inheritdoc/>
        public long Overspending
            => this.Balance!.Overspending;

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            this.Balance!.Topup(delta);

            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
            {
                this.Expiry?.Renew();
            }
        }

        /// <inheritdoc/>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            var result = this.Balance!.Deduct(delta, policy);

            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
            {
                this.Expiry?.Renew();
            }

            return result;
        }

        /// <inheritdoc/>
        public void Adjust(long? incoming, long? outgoing)
        {
            this.Balance!.Adjust(incoming, outgoing);
            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.Expiry?.Renew();
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.Balance!.Clear();
            if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
            {
                this.Expiry?.Renew();
            }
        }
    }
}
