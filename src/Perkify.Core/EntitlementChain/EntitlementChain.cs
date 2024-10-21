// <copyright file="EntitlementChain.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using System.Collections.Immutable;
    using NodaTime;

    using EntitlementComparer = System.Collections.Generic.IComparer<Perkify.Core.Entitlement>;
    using EntitlementFactory = System.Func<long, System.DateTime?, NodaTime.IClock, Perkify.Core.Entitlement>;

    /// <summary>
    /// The entitlement chain for eligibility.
    /// </summary>
    /// <param name="policy">The policy to be applied to the entitlement chain.</param>
    /// <param name="clock">The clock used to determine the current time.</param>
    public partial class EntitlementChain(EntitlementChainPolicy policy = EntitlementChainPolicy.Default, IClock? clock = null)
    {
        /// <summary>
        /// The default factory function to build new entitlements.
        /// </summary>
        public static readonly EntitlementFactory DefaultEntitlementFactory = (delta, expiryUtc, clock) =>
        {
            var balance = Balance.Debit(BalanceExceedancePolicy.Overflow);
            var expiry = expiryUtc != null ? new Expiry(expiryUtc.Value, clock).WithRenewal("P90D!") : null;
            var enablement = new Enablement(true, clock);
            var entitlement = new Entitlement(AutoRenewalMode.Default, clock)
            {
                Balance = balance,
                Expiry = expiry,
                Enablement = enablement,
            };
            entitlement.Topup(delta);
            return entitlement;
        };

        /// <summary>
        /// The default comparer used to sort entitlements by their expiry date in UTC.
        /// </summary>
        public static readonly EntitlementComparer DefaultEntitlementComparer = new EntitlementExpiryUtcComparer();

        // The collection of entitlements in the chain.
        private ImmutableSortedSet<Entitlement> entitlements = ImmutableSortedSet<Entitlement>.Empty;

        /// <summary>
        /// Gets the policy applied to the entitlement chain.
        /// </summary>
        public EntitlementChainPolicy EntitlementChainPolicy => policy;

        /// <summary>
        /// Gets the clock used to determine the current time.
        /// </summary>
        public IClock Clock { get; private set; } = clock ?? SystemClock.Instance;

        /// <summary>
        /// Gets the factory function to build new entitlements.
        /// </summary>
        public EntitlementFactory Factory { get; init; } = DefaultEntitlementFactory;

        /// <summary>
        /// Gets the comparer used to sort entitlements.
        /// </summary>
        public EntitlementComparer Comparer { get; init; } = DefaultEntitlementComparer;

        /// <summary>
        /// Gets the collection of entitlements in the chain.
        /// </summary>
        public IEnumerable<Entitlement> Entitlements
        {
            get => this.entitlements;
            init => this.entitlements = value?.Select(entitlement => entitlement.WithClock(clock)).ToImmutableSortedSet(this.Comparer) ?? ImmutableSortedSet<Entitlement>.Empty;
        }

        /// <summary>
        /// Sets a new clock for the entitlement chain and updates all entitlements with the new clock.
        /// </summary>
        /// <param name="clock">The new clock to be set.</param>
        /// <returns>The updated entitlement chain with the new clock.</returns>
        public EntitlementChain WithClock(IClock? clock)
        {
            clock ??= SystemClock.Instance;
            this.entitlements.ToList().ForEach(entitlement => entitlement.WithClock(clock));
            this.Clock = clock;
            return this;
        }
    }
}
