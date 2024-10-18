// <copyright file="EntitlementChain.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class EntitlementChain : IBalance
    {
        /// <inheritdoc/>
        public long Incoming => this.entitlements.Sum(entitlement => entitlement.Incoming);

        /// <inheritdoc/>
        public long Outgoing => this.entitlements.Sum(entitlement => entitlement.Outgoing);

        /// <inheritdoc/>
        public long Threshold => this.entitlements.Sum(entitlement => entitlement.Threshold);

        /// <inheritdoc/>
        public BalanceType BalanceType
            => this.entitlements.Any(entitlement => entitlement.BalanceType == BalanceType.Credit)
                ? BalanceType.Credit
                : BalanceType.Debit;

        /// <inheritdoc/>
        public long Gross => this.entitlements.Sum(entitlement => entitlement.Gross);

        /// <inheritdoc/>
        public long Overspending => this.entitlements.Sum(entitlement => entitlement.Gross);

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            var entitlement = this.Factory.Invoke(delta, this.Clock.GetCurrentInstant().ToDateTimeUtc());
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
        public void Adjust(long? incoming, long? outgoing)
        {
            throw new NotSupportedException("Please adjust specific entitlement or its balance.");
        }

        /// <inheritdoc/>
        public void Clear()
            => this.entitlements.ToList().ForEach(entitlement => entitlement.Clear());
    }
}
