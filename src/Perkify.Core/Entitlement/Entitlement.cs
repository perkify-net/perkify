// <copyright file="Entitlement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// Represents an entitlement with balance and expiry management.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Entitlement"/> class with auto-renewal mode.
    /// </remarks>
    /// <param name="autorenewal">The auto-renewal mode.</param>
    /// <param name="clock">The clock instance used to retrieve the current time.</param>
    public partial class Entitlement(AutoRenewalMode autorenewal = AutoRenewalMode.Default, IClock? clock = null)
        : IEligible
    {
        private Balance? balance;
        private Enablement? enablement;
        private Expiry? expiry;

        /// <summary>
        /// Gets the clock instance used to retrieve the current time.
        /// </summary>
        /// <remarks>
        /// The clock is shared used across expiry and activation by default.
        /// </remarks>
        public IClock Clock => clock ?? SystemClock.Instance;

        /// <summary>
        /// Gets the balance associated with the entitlement.
        /// </summary>
        /// <remarks>
        /// Do not use. <see cref="InvalidOperationException"/> will be thrown to prevent state mutation with synchronization issues.
        /// </remarks>
        public Balance? Balance
        {
            // NOTE:
            // - Do not expose inner object to prevent state mutation with synchronization issues.
            // - Another solution is to use private getter, but it will raise CA1822 warning.
            get => throw new InvalidOperationException("Access denied (inner balance).");
            init => this.balance = value;
        }

        /// <summary>
        /// Gets a value indicating whether the entitlement has an embedded balance.
        /// </summary>
        public virtual bool HasBalance => this.balance != null;

        /// <summary>
        /// Gets the expiry associated with the entitlement.
        /// </summary>
        /// <remarks>
        /// Do not use. <see cref="InvalidOperationException"/> will be thrown to prevent state mutation with synchronization issues.
        /// </remarks>
        public Expiry? Expiry
        {
            // NOTE:
            // - Do not expose inner object to prevent state mutation with synchronization issues.
            // - Another solution is to use private getter, but it will raise CA1822 warning.
            get => throw new InvalidOperationException("Access denied (inner expiry).");
            init
            {
                this.expiry = value;

                if (this.expiry != null)
                {
                    this.expiry.Clock = this.Clock;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entitlement has an embedded expiry.
        /// </summary>
        public virtual bool HasExpiry => this.expiry != null;

        /// <summary>
        /// Gets the enablement associated with the entitlement.
        /// </summary>
        /// <remarks>
        /// Do not use. <see cref="InvalidOperationException"/> will be thrown to prevent state mutation with synchronization issues.
        /// </remarks>
        public Enablement? Enablement
        {
            // NOTE:
            // - Do not expose inner object to prevent state mutation with synchronization issues.
            // - Another solution is to use private getter, but it will raise CA1822 warning.
            get => throw new InvalidOperationException("Access denied (inner enablement).");
            init
            {
                this.enablement = value;

                if (this.enablement != null)
                {
                    this.enablement.Clock = this.Clock;
                    this.enablement.EnablementStateChanged += (sender, args) => this.EnablementStateChanged?.Invoke(this, args);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entitlement has an embedded enablement.
        /// </summary>
        public virtual bool HasEnablement => this.enablement != null;

        /// <summary>
        /// Gets the prerequisite eligibility associated with the entitlement.
        /// </summary>
        public IEligible? Prerequesite { get; init; }

        /// <summary>
        /// Gets the auto-renewal mode connecting balance and expiry.
        /// </summary>
        public AutoRenewalMode AutoRenewalMode { get; } = autorenewal;

        /// <inheritdoc/>
        public virtual bool IsEligible =>
            (this.balance?.IsEligible ?? true)
            && (this.expiry?.IsEligible ?? true)
            && (this.enablement?.IsEligible ?? true)
            && (this.Prerequesite?.IsEligible ?? true);
    }
}
