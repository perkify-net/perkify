// <copyright file="Entitlement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents an entitlement with balance and expiry management.
    /// </summary>
    public partial class Entitlement : IEligible
    {
        private readonly Balance balance;
        private readonly Expiry? expiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entitlement"/> class with balance, expiry, and auto-renewal mode.
        /// </summary>
        /// <param name="balance">The balance associated with the entitlement.</param>
        /// <param name="expiry">The expiry associated with the entitlement.</param>
        /// <param name="mode">The auto-renewal mode.</param>
        public Entitlement(Balance balance, Expiry expiry, AutoRenewalMode mode = AutoRenewalMode.Default)
        {
            this.balance = balance;
            this.expiry = expiry;
            this.AutoRenewalMode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entitlement"/> class with balance and default auto-renewal mode.
        /// </summary>
        /// <param name="balance">The balance associated with the entitlement.</param>
        public Entitlement(Balance balance)
        {
            this.balance = balance;
            this.expiry = null;
            this.AutoRenewalMode = AutoRenewalMode.None;
        }

        /// <summary>
        /// Gets the auto-renewal mode.
        /// </summary>
        public AutoRenewalMode AutoRenewalMode { get; private set; }

        /// <summary>
        /// Gets the prerequisite eligibility.
        /// </summary>
        public IEligible? Prerequesite { get; init; }

        /// <inheritdoc/>
        public bool IsEligible =>
            (this.balance?.IsEligible ?? true)
            && (this.expiry?.IsEligible ?? true)
            && (this.Prerequesite?.IsEligible ?? true);
    }
}
