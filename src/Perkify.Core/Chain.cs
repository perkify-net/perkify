// <copyright file="Chain.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The entitlement chain for eligibility.
    /// </summary>
    public class Chain : IEligible, IBalance<Chain>
    {
        private readonly List<Entitlement> entitlements;

        private readonly Func<long, Entitlement> entitlementBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chain"/> class.
        /// </summary>
        /// <param name="entitlements">The collection of entitlements to initialize the chain with.</param>
        /// <param name="entitlementBuilder">The function to build new entitlements.</param>
        public Chain(IEnumerable<Entitlement>? entitlements = null, Func<long, Entitlement>? entitlementBuilder = null)
        {
            entitlements ??= [];
            this.entitlements = [.. entitlements.OrderBy(entitlement => entitlement.ExpiryUtc)];
            entitlementBuilder ??= (delta) => new Entitlement(new Balance(delta));
            this.entitlementBuilder = entitlementBuilder;
        }

        /// <inheritdoc/>
        public bool IsEligible => this.entitlements.Any(entitlement => entitlement.IsEligible);

        #region Implements IBalance<T> interface

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

        #endregion
    }
}
