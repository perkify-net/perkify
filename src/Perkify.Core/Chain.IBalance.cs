// <copyright file="Chain.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Chain : IBalance<Chain>
    {
        /// <inheritdoc/>
        public long Incoming => this.entitlements.Sum(entitlement => entitlement.Incoming);

        /// <inheritdoc/>
        public long Outgoing => this.entitlements.Sum(entitlement => entitlement.Outgoing);

        /// <inheritdoc/>
        public long Threshold => this.entitlements.Sum(entitlement => entitlement.Threshold);

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            var entitlement = this.entitlementBuilder.Invoke(delta);
            this.entitlements.Add(entitlement);
        }

        /// <inheritdoc/>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            var entitlements = this.entitlements
                .Where(entitlement => entitlement.IsEligible)
                .OrderBy(entitlement => entitlement.ExpiryUtc)
                .ToList();

            if (entitlements.Count == 0)
            {
                throw new InvalidOperationException("Ineligible state.");
            }

            foreach (var entitlement in entitlements)
            {
                var processed = delta;
                delta = entitlement.Deduct(processed, policy);
                if (delta == 0)
                {
                    break;
                }
            }

            return delta;
        }

        /// <inheritdoc/>
        public Chain Adjust(long? incoming, long? outgoing)
        {
            throw new InvalidOperationException("Please adjust in specific entitlement or balance.");
        }

        /// <inheritdoc/>
        public Chain Clear()
        {
            this.entitlements.ForEach(entitlement => entitlement.Clear());
            return this;
        }
    }
}
