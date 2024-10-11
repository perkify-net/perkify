// <copyright file="Chain.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The entitlement chain for eligibility.</summary>
    public class Chain : IEligible, IBalance<Chain>
    {
        private readonly List<Entitlement> entitlements;

        private readonly Func<long, Entitlement> entitlementBuilder;

        public Chain(IEnumerable<Entitlement>? entitlements = null, Func<long, Entitlement>? entitlementBuilder = null)
        {
            entitlements ??= Enumerable.Empty<Entitlement>();
            this.entitlements =[.. entitlements.OrderBy(entitlement => entitlement.ExpiryUtc)];

            entitlementBuilder ??= (delta) => new Entitlement(new Balance(delta));
            this.entitlementBuilder = entitlementBuilder;
        }

        #region Implement IEligible interface

        public bool IsEligible => this.entitlements.Any(entitlement => entitlement.IsEligible);

        #endregion

        #region Implements IBalance<T> interface

        public long Incoming => this.entitlements.Sum(entitlement => entitlement.Incoming);

        public long Outgoing => this.entitlements.Sum(entitlement => entitlement.Outgoing);

        public long Threshold => this.entitlements.Sum(entitlement => entitlement.Threshold);

        public void Topup(long delta)
        {
            var entitlement = this.entitlementBuilder.Invoke(delta);
            this.entitlements.Add(entitlement);
        }

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

        public Chain Adjust(long? incoming, long? outgoing)
        {
            throw new InvalidOperationException("Please adjust in specific entitlement or balance.");
        }

        #endregion
    }
}
