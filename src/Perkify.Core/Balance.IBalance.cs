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
        public long Threshold { get; private set; } = threshold;

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
            if (incoming.HasValue && incoming.Value < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(incoming), "The incoming/outgoing amount must be null or greater than or equal to 0");
            }

            if (outgoing.HasValue && outgoing.Value < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(outgoing), "The incoming/outgoing amount must be null or greater than or equal to 0");
            }

            this.Incoming = incoming ?? this.Incoming;
            this.Outgoing = outgoing ?? this.Outgoing;
            return this;
        }
    }
}
