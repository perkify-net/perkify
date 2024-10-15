// <copyright file="Enablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The activator managing activation and deactivation for eligibility.
    /// </summary>
    public partial class Enablement(bool isActive = true)
        : INowUtc, IEligible
    {
        /// <summary>
        /// Gets or sets the clock instance used to retrieve the current time.
        /// </summary>
        public IClock Clock { get; set; } = SystemClock.Instance;

        /// <inheritdoc/>
        public DateTime NowUtc => this.Clock.GetCurrentInstant().ToDateTimeUtc();

        /// <inheritdoc/>
        public bool IsEligible =>
            this.IsImmediateEffective || this.NowUtc < this.EffectiveUtc
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
