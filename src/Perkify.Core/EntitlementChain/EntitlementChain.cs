// <copyright file="EntitlementChain.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using System.Collections.Immutable;
    using NodaTime;

    using EntitlementComparer = System.Collections.Generic.IComparer<Perkify.Core.Entitlement>;
    using EntitlementFactory = System.Func<long, System.DateTime, Perkify.Core.Entitlement>;

    /// <summary>
    /// The entitlement chain for eligibility.
    /// </summary>
    /// <param name="factory">The function to build new entitlements.</param>
    /// <param name="comparer">The comparer used to sort entitlements.</param>
    /// <param name="clock">The clock used to determine the current time.</param>
    public partial class EntitlementChain(EntitlementFactory? factory = null, EntitlementComparer? comparer = null, IClock? clock = null)
    {
        private ImmutableSortedSet<Entitlement> entitlements = ImmutableSortedSet<Entitlement>.Empty;

        /// <summary>
        /// Gets the factory function to build new entitlements.
        /// </summary>
        public EntitlementFactory Factory { get; } = factory ?? new EntitlementFactory((delta, expiryUtc) => new Entitlement(AutoRenewalMode.Default)
        {
            Balance = new Balance(delta, BalanceExceedancePolicy.Reject),
            Expiry = new Expiry(expiryUtc, null),
            Enablement = new Enablement(true),
        });

        /// <summary>
        /// Gets the comparer used to sort entitlements.
        /// </summary>
        public EntitlementComparer Comparer { get; } = comparer ?? new DefaultEntitlementComparer();

        /// <summary>
        /// Gets the collection of entitlements in the chain.
        /// </summary>
        public IEnumerable<Entitlement> Entitlements
        {
            get => this.entitlements;
            init => this.entitlements = value?.ToImmutableSortedSet(this.Comparer) ?? ImmutableSortedSet<Entitlement>.Empty;
        }

        /// <summary>
        /// Gets the clock used to determine the current time.
        /// </summary>
        public IClock Clock { get; } = clock ?? SystemClock.Instance;
    }
}
