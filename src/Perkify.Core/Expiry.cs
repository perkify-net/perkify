// <copyright file="Expiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>
    /// Expiry time for eligibility.
    /// </summary>
    public partial class Expiry : IEligible, INowUtc
    {
        private readonly IClock clock;

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
        public DateTime NowUtc => this.clock.GetCurrentInstant().ToDateTimeUtc();

        /// <inheritdoc/>
        public bool IsEligible => (this.suspensionUtc, this.NowUtc, this.GetDeadlineUtc()) switch
        {
            // Not suspended
            (null, DateTime n, DateTime d) => n < d,

            // Already suspended
            (DateTime s, DateTime n, DateTime d) when s <= n => false,

            // Will suspend in future
            (DateTime s, DateTime n, DateTime d) when s > n => n < d,

            // Default
            _ => throw new InvalidOperationException("Invalid state.")
        };

        /// <summary>Specify the suspension time.</summary>
        /// <param name="suspensionUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        public Expiry WithSuspensionUtc(DateTime suspensionUtc)
        {
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
