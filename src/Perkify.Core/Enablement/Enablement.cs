// <copyright file="Enablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// The activator managing activation and deactivation for eligibility.
    /// </summary>
    /// <param name="deactivationUtc">The UTC time when deactivation should occur.</param>
    public partial class Enablement(DateTime? deactivationUtc = null)
        : INowUtc, IEligible
    {
        /// <summary>
        /// Gets or sets the clock instance used to retrieve the current time.
        /// </summary>
        public IClock Clock { get; set; } = SystemClock.Instance;

        /// <inheritdoc/>
        public DateTime NowUtc => this.Clock.GetCurrentInstant().ToDateTimeUtc();

        /// <inheritdoc/>
        public bool IsEligible => this.IsActive switch
        {
            true => true,
            false => this.NowUtc < this.DeactivationUtc!.Value
        };
    }
}
