// <copyright file="Expiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <summary>Expiry time for eligibility.</summary>
    public class Expiry : INowUtc, IEligible, IExpiry<Expiry>
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

        #region Implements INowUtc interface

        private readonly IClock clock;

        /// <inheritdoc/>
        public DateTime NowUtc => this.clock.GetCurrentInstant().ToDateTimeUtc();

        #endregion

        #region Implements IEligible interface

        /// <inheritdoc/>
        public bool IsEligible => !this.suspensionUtc.HasValue && this.NowUtc < this.GetDeadlineUtc();

        #endregion

        #region Implements IExpiry<T> interface

        #region IsExpired, Remaining & Overdue

        /// <inheritdoc/>
        public TimeSpan GracePeriod { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Remaining
        {
            get
            {
                if (this.suspensionUtc.HasValue)
                {
                    return this.ExpiryUtc - this.suspensionUtc.Value;
                }
                else
                {
                    var nowUtc = this.NowUtc;
                    return nowUtc <= this.GetDeadlineUtc() ? this.ExpiryUtc - nowUtc : this.GracePeriod.Negate();
                }
            }
        }

        /// <inheritdoc/>
        public TimeSpan Overdue => this.suspensionUtc.HasValue
            ? this.suspensionUtc.Value > this.ExpiryUtc ? this.suspensionUtc.Value - this.ExpiryUtc : TimeSpan.Zero
            : this.NowUtc switch
            {
                var nowUtc when nowUtc <= this.ExpiryUtc => TimeSpan.Zero,
                var nowUtc when nowUtc < this.GetDeadlineUtc() => nowUtc - this.ExpiryUtc,
                var _ => this.GracePeriod
            };

        #endregion

        #region Renew

        /// <inheritdoc/>
        public DateTime ExpiryUtc { get; private set; }

        /// <inheritdoc/>
        public Renewal? Renewal { get; private set; }

        /// <inheritdoc/>
        public Expiry Renew(Renewal? renewal = null)
        {
            renewal ??= this.Renewal;
            if (renewal == null)
            {
                throw new ArgumentNullException(nameof(renewal), "Renewal period is required.");
            }

            if (!this.IsActive)
            {
                throw new InvalidOperationException("Suspended state.");
            }

            var previousExpiryUtc = this.ExpiryUtc;
            var nextExpiryUtc = renewal.Renew(previousExpiryUtc);
            if (nextExpiryUtc <= previousExpiryUtc)
            {
                throw new InvalidOperationException("Negative ISO8601 duration.");
            }

            this.ExpiryUtc = nextExpiryUtc;
            this.Renewal = renewal;
            return this;
        }

        #endregion

        #region Deactivate & Activate

        private DateTime? suspensionUtc;

        /// <inheritdoc/>
        public DateTime? SuspensionUtc => this.suspensionUtc ?? (this.IsEligible ? null : this.GetDeadlineUtc());

        /// <inheritdoc/>
        public bool IsActive => !this.suspensionUtc.HasValue;

        /// <inheritdoc/>
        public Expiry Deactivate(DateTime? suspensionUtc = null)
        {
            // Keep idempotent if resubmitting suspending requests.
            if (this.suspensionUtc.HasValue)
            {
                return this;
            }

            suspensionUtc ??= this.ExpiryUtc;
            var deadlineUtc = this.GetDeadlineUtc();
            this.suspensionUtc = suspensionUtc < deadlineUtc ? suspensionUtc : deadlineUtc;
            return this;
        }

        /// <inheritdoc/>
        public Expiry Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            if (!this.suspensionUtc.HasValue)
            {
                return this;
            }

            var finalResumptionUtc = resumptionUtc ??= this.NowUtc;
            if (finalResumptionUtc < this.suspensionUtc.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(resumptionUtc), "Resume time must be greater than suspend time.");
            }

            if (extended)
            {
                this.ExpiryUtc = finalResumptionUtc + this.Remaining;
            }

            this.suspensionUtc = null;
            return this;
        }

        #endregion

        #endregion
    }
}
