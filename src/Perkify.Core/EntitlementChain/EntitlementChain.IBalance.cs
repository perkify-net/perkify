// <copyright file="EntitlementChain.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class EntitlementChain : IBalance
    {
        /// <inheritdoc/>
        public long Threshold
            => this.entitlements
                .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
                .Sum(entitlement => entitlement.Threshold);

        /// <inheritdoc/>
        public BalanceExceedancePolicy BalanceExceedancePolicy
            => this.entitlements
                .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
                .Select(entitlement => entitlement.BalanceExceedancePolicy)
                .DefaultIfEmpty(BalanceExceedancePolicy.Reject)
                .Max();

        /// <inheritdoc/>
        public BalanceType BalanceType
            => this.entitlements
                .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
                .Select(entitlement => entitlement.BalanceType)
                .DefaultIfEmpty(BalanceType.Debit)
                .Max();

        /// <inheritdoc/>
        public long Incoming => this.entitlements
            .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
            .Sum(entitlement => entitlement.Incoming);

        /// <inheritdoc/>
        public long Outgoing => this.entitlements
            .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
            .Sum(entitlement => entitlement.Outgoing);

        /// <inheritdoc/>
        public long Gross => this.entitlements
            .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
            .Sum(entitlement => entitlement.Gross);

        /// <inheritdoc/>
        public long Available => this.entitlements
            .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
            .Sum(entitlement => entitlement.Available);

        /// <inheritdoc/>
        public long Overspending => this.entitlements
            .Where(entitlement => !this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.EligibleOnlyView) || entitlement.IsEligible)
            .Sum(entitlement => entitlement.Overspending);

        /// <inheritdoc/>
        public void Topup(long delta)
        {
            var nowUtc = this.Clock.GetCurrentInstant().ToDateTimeUtc();
            var expiryUtc = this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.WithAutoRenewalExpiry) ? nowUtc : (DateTime?)null;
            var entitlement = this.Factory.Invoke(delta, expiryUtc, this.Clock);
            this.entitlements = this.entitlements.Add(entitlement);
        }

        /// <inheritdoc/>
        public long Deduct(long delta)
        {
            var available = this.entitlements
                .Where(entitlement => entitlement.IsEligible)
                .Where(entitlement => entitlement.BalanceExceedancePolicy != BalanceExceedancePolicy.Reject || entitlement.Available >= delta)
                .ToList();

            if (available.Count == 0)
            {
                throw new InvalidOperationException("No availble entitlement.");
            }

            if (this.EntitlementChainPolicy.HasFlag(EntitlementChainPolicy.SplitDeductionAllowed))
            {
                available.ForEach(entitlement => delta = delta > 0 ? entitlement.Deduct(delta) : delta);
                return delta;
            }
            else
            {
                // TODO: Refactor
                var entitlement = available.First(entitlement => entitlement.BalanceExceedancePolicy.GetDeductibleAllowance(entitlement.Gross, entitlement.Threshold) >= delta);
                return entitlement.Deduct(delta);
            }
        }

        /// <inheritdoc/>
        public void Adjust(long? incoming, long? outgoing)
            => throw new NotSupportedException("Please adjust specific entitlement or its balance.");

        /// <inheritdoc/>
        public void Clear()
            => this.entitlements.ToList().ForEach(entitlement => entitlement.Clear());
    }
}
