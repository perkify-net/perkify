﻿// <copyright file="Expiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// Expiry time for eligibility.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Expiry"/> class.
    /// Create the expiry time for eligibility.
    /// </remarks>
    /// <param name="expiryUtc">Expiry time in UTC.</param>
    /// <param name="grace">Grace period.</param>
    public partial class Expiry(DateTime expiryUtc, TimeSpan? grace = null)
        : IEligible, INowUtc
    {
        /// <summary>
        /// Gets or sets the clock instance used to retrieve the current time.
        /// </summary>
        public IClock Clock { get; set; } = SystemClock.Instance;

        /// <inheritdoc/>
        public DateTime NowUtc => this.Clock.GetCurrentInstant().ToDateTimeUtc();

        /// <inheritdoc/>
        public bool IsEligible => this.NowUtc < this.DeadlineUtc;

        /// <summary>Specify the renewal period.</summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        /// <returns>The expiry time with specified renewal period.</returns>
        public Expiry WithRenewal(ChronoInterval renewal)
        {
            this.Renewal = renewal;
            return this;
        }
    }
}
