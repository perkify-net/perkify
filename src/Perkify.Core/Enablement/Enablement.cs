// <copyright file="Enablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// The activator managing activation and deactivation for eligibility.
    /// </summary>
    public partial class Enablement : INowUtc, IEligible
    {
        /// <summary>
        /// Gets or sets the clock instance used to retrieve the current time.
        /// </summary>
        public IClock Clock { get; set; } = SystemClock.Instance;

        /// <inheritdoc/>
        public DateTime NowUtc => this.Clock.GetCurrentInstant().ToDateTimeUtc();

        /// <inheritdoc/>
        public bool IsEligible => this.IsActive || this.NowUtc < this.DeactivationUtc;

        /// <summary>
        /// Initialize with the activation time.
        /// </summary>
        /// <param name="deactivationUtc">The UTC time when deactivation should occur.</param>
        /// <returns>The current instance of the <see cref="Enablement"/> class with updated deactivation time.</returns>
        public Enablement WithDeactivationUtc(DateTime deactivationUtc)
        {
            this.DeactivationUtc = deactivationUtc;
            return this;
        }
    }
}
