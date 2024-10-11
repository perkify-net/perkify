// <copyright file="Expiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// Expiry time for eligibility.
    /// </summary>
    public partial class Expiry : IEligible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expiry"/> class.Create the expiry time for eligibility.</summary>
        /// <param name="expiryUtc">Expiry time in UTC.</param>
        /// <param name="grace">Grace period.</param>
        /// <param name="clock">System clock abstraction for testability.</param>
        public Expiry(DateTime? expiryUtc = null, TimeSpan? grace = null, IClock? clock = null)
        {
            clock ??= SystemClock.Instance;
            this.ExpiryUtc = expiryUtc ?? clock.GetCurrentInstant().ToDateTimeUtc();
            this.GracePeriod = grace ?? TimeSpan.Zero;
            this.Renewal = null;
            this.clock = clock;
            this.suspensionUtc = null;
        }

        /// <inheritdoc/>
        public bool IsEligible => !this.suspensionUtc.HasValue && this.NowUtc < this.GetDeadlineUtc();

        /// <summary>Specify the suspension time.</summary>
        /// <param name="suspensionUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="InvalidOperationException">Resuspending is not allowed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier with the current time.</exception>
        public Expiry WithSuspensionUtc(DateTime suspensionUtc)
        {
            if (this.suspensionUtc.HasValue)
            {
                throw new InvalidOperationException("Resuspending is not allowed.");
            }

            if (suspensionUtc > this.NowUtc)
            {
                throw new ArgumentOutOfRangeException(nameof(suspensionUtc), "The suspension time must be earlier with the current time");
            }

            this.suspensionUtc = suspensionUtc < this.GetDeadlineUtc() ? suspensionUtc : this.GetDeadlineUtc();
            return this;
        }

        /// <summary>Specify the renewal period.</summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        /// <returns>The expiry time with specified renewal period.</returns>
        public Expiry WithRenewal(Renewal renewal)
        {
            this.Renewal = renewal;
            return this;
        }
    }
}
