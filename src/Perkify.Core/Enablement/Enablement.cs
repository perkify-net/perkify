// <copyright file="Enablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// The activator managing activation and deactivation for eligibility.
    /// </summary>
    public partial class Enablement(bool isActive = true, IClock? clock = null)
        : IEligible
    {
        /// <summary>
        /// Gets or sets the clock instance used to retrieve the current time.
        /// </summary>
        public IClock Clock { get; set; } = clock ?? SystemClock.Instance;

        /// <inheritdoc/>
        public virtual bool IsEligible =>
            this.IsImmediateEffective || this.Clock.GetCurrentInstant().ToDateTimeUtc() < this.EffectiveUtc
                ? this.IsActive
                : !this.IsActive;

        /// <summary>
        /// Sets the effective date and time in UTC and whether it is immediately effective.
        /// </summary>
        /// <param name="effectiveUtc">The effective date and time in UTC.</param>
        /// <param name="isImmediateEffective">Indicates if the change is immediately effective.</param>
        /// <returns>The updated <see cref="Enablement"/> instance.</returns>
        public Enablement WithEffectiveUtc(DateTime effectiveUtc, bool isImmediateEffective)
        {
            this.EffectiveUtc = effectiveUtc;
            this.IsImmediateEffective = isImmediateEffective;
            return this;
        }
    }
}
