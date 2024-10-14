// <copyright file="Chain.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The entitlement chain for eligibility.
    /// </summary>
    public partial class Chain : IEligible
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
            entitlements ??= Enumerable.Empty<Entitlement>();
            this.entitlements = entitlements.OrderBy(entitlement => entitlement.ExpiryUtc).ToList();
            Func<long, Entitlement> defaultEntitlementBuilder = (delta) => new Entitlement(AutoRenewalMode.Default)
            {
                Balance = new Balance(delta, BalanceExceedancePolicy.Reject),
            };
            this.entitlementBuilder = entitlementBuilder ?? defaultEntitlementBuilder;
        }

        /// <inheritdoc/>
        public bool IsEligible => this.entitlements.Any(entitlement => entitlement.IsEligible);
    }
}
