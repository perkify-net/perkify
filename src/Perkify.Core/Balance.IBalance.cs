// <copyright file="Balance.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Balance : IBalance<Balance>
    {
        /// <inheritdoc/>
        public long Incoming { get; private set; }

        /// <inheritdoc/>
        public long Outgoing { get; private set; }

        /// <inheritdoc/>
        public long Threshold { get; } = threshold;

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be greater than 0");
            }

            checked
            {
                this.Incoming += delta;
            }
        }

        /// <inheritdoc/>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "The Amount must be greater than 0.");
            }

            if (!this.IsEligible)
            {
                throw new InvalidOperationException("Ineligible state.");
            }

            var maximum = this.GetMaxDeductibleAmount(policy);
            var processed = delta;
            var remaining = policy.Deduct(ref processed, maximum);
            checked
            {
                this.Outgoing += processed;
            }

            return remaining;
        }

        /// <inheritdoc/>
        public Balance Adjust(long? incoming, long? outgoing)
        {
            checked
            {
                this.Incoming += incoming ?? 0;
                this.Outgoing += outgoing ?? 0;
            }

            return this;
        }

        /// <inheritdoc/>
        public Balance Clear()
        {
            this.Incoming = 0;
            this.Outgoing = 0;
            return this;
        }
    }
}
