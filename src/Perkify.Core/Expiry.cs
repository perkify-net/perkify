namespace Perkify.Core
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Expiry time for eligibility.
    /// </summary>
    public class Expiry : IExpiry, IEligible
    {
        #region System Clock Dependency

        private readonly IClock clock;

        public DateTime NowUtc => this.clock.GetCurrentInstant().ToDateTimeUtc();

        #endregion

        /// <summary>
        /// The expiry time in UTC.
        /// </summary>
        public DateTime ExpiryUtc { get; private set; }

        /// <summary>
        /// Grace period.
        /// </summary>
        public TimeSpan GracePeriod { get; private set; }

        /// <summary>
        /// The deadline time in UTC.
        /// </summary>
        public DateTime DeadlineUtc => this.ExpiryUtc + this.GracePeriod;

        /// <summary>
        /// Create the expiry time for eligibility.
        /// </summary>
        /// <param name="expiryUtc">Expiry time in UTC.</param>
        /// <param name="grace">Grace period.</param>
        /// <param name="clock">System clock abstraction for testability.</param>
        public Expiry(DateTime? expiryUtc = null, TimeSpan? grace = null, IClock? clock = null)
        {
            clock ??= SystemClock.Instance;
            this.ExpiryUtc = expiryUtc ?? clock.GetCurrentInstant().ToDateTimeUtc();
            this.GracePeriod = grace ?? TimeSpan.Zero;
            this.clock = clock;
            this.suspensionUtc = null;
        }

        /// <summary>
        /// Specify the suspension time.
        /// </summary>
        /// <param name="suspensionUtc"></param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="InvalidOperationException">Resuspending is not allowed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier with the current time.</exception>
        public Expiry WithSuspensionUtc(DateTime suspensionUtc)
        {
            if(this.suspensionUtc.HasValue)
            {
                throw new InvalidOperationException("Resuspending is not allowed.");
            }

            if(suspensionUtc > this.NowUtc)
            {
                throw new ArgumentOutOfRangeException(nameof(suspensionUtc), "The suspension time must be earlier with the current time"); 
            }

            this.suspensionUtc = suspensionUtc < this.DeadlineUtc ? suspensionUtc : this.DeadlineUtc;
            return this;
        }

        #region Implements IEligible interface

        /// <summary>
        /// See also in IEligible interface.
        /// </summary>
        public bool IsEligible => !this.suspensionUtc.HasValue && this.NowUtc < this.DeadlineUtc;

        #endregion

        #region Expired, Remaining & Overdue

        /// <summary>
        /// Boolean flag to identify if the expiry time is expired.
        /// </summary>
        public bool Expired => (this.suspensionUtc ?? this.NowUtc) >= this.ExpiryUtc;

        /// <summary>
        /// The remaining portion.
        /// - If suspended, the remaining portion is the time between suspend time and expiry time.
        /// - If eligible, the remaining portion is the current time between now and expiry time.
        /// - If ineligible (after grace period), the remaining portion is negative grace period.
        /// </summary>
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
                    return nowUtc <= this.DeadlineUtc ? this.ExpiryUtc - nowUtc : this.GracePeriod.Negate();
                }
            }
        }

        /// <summary>
        /// The overdue portion.
        /// - Suspended: The overdue portion is the time between suspend time and expiry time. Zero if suspend time is earlier than expiry time.
        /// - Eligible: The overdue portion is the time between expiry time and now. Zero if now is earlier than expiry time.
        /// - Ineligible (after grace period): The overdue portion is the grace period.
        /// </summary>
        public TimeSpan Overdue => this.suspensionUtc.HasValue
            ? this.suspensionUtc.Value > this.ExpiryUtc ? this.suspensionUtc.Value - this.ExpiryUtc : TimeSpan.Zero
            : this.NowUtc switch
            {
                var nowUtc when nowUtc <= this.ExpiryUtc => TimeSpan.Zero,
                var nowUtc when nowUtc < this.DeadlineUtc => nowUtc - this.ExpiryUtc,
                var _ => this.GracePeriod
            };

        #endregion

        #region Operations

        #region Renew

        /// <summary>
        /// Renew the expiry time with a positive time span.
        /// </summary>
        /// <param name="ts">The time span to renew the expiry time.</param>
        /// <returns>The expiry time after renewal.</returns>
        /// <remarks>The renewval time span is not ISO8601 compatible. The day of month and year are not well considered which may cause inaccurate expiry time after renewal.</remarks>
        public Expiry Renew(TimeSpan ts)
        {
            if (ts.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ts), "Negative time span.");
            }

            if (!this.IsActive)
            {
                throw new InvalidOperationException("Suspended state.");
            }

            this.ExpiryUtc += ts;
            return this;
        }

        /// <summary>
        /// Renew the expiry time in timeline arithmetic or calendrical arithmetic.
        /// </summary>
        /// <param name="duration">Time span in ISO8601 duration format (string).</param>
        /// <param name="calendar">Boolean flag to identify if extending expiry time in calendar system.</param>
        /// <returns>The expiry time after renewal.</returns>
        public Expiry Renew(string duration, bool calendar = false)
        {
            var result = PeriodPattern.NormalizingIso.Parse(duration);
            if (!result.Success)
            {
                throw new FormatException("Incorrect ISO8601 duration string.", result.Exception);
            }

            var period = result.Value.Normalize();
            if (!calendar)
            {
                var ts = period.ToDuration().ToTimeSpan();
                return this.Renew(ts);
            }

            if (!this.IsActive)
            {
                throw new InvalidOperationException("Suspended state.");
            }

            var current = Instant.FromDateTimeUtc(this.ExpiryUtc).InZone(DateTimeZone.Utc).LocalDateTime;
            var future = current.Plus(period);
            if (current > future)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Negative ISO8601 duration.");
            }

            this.ExpiryUtc = future.InZoneStrictly(DateTimeZone.Utc).ToInstant().ToDateTimeUtc();
            return this;
        }

        #endregion

        #region Suspend & Resume

        private DateTime? suspensionUtc;

        /// <summary>
        /// The suspend time in UTC.
        /// - Explicit Suspension Time (Suspend)
        /// - Implicit Suspension Time (Deadline) when the expiry time is not eligible.
        /// - Null when the expiry time is eligible.
        /// </summary>
        public DateTime? SuspensionUtc => this.suspensionUtc ?? (this.IsEligible ? null : this.DeadlineUtc);

        /// <summary>
        /// Boolean flag to identify if the expiry time is suspended.
        /// </summary>
        public bool IsActive => !this.suspensionUtc.HasValue;

        /// <summary>
        /// Suspends the expiry time.
        /// </summary>
        /// <param name="suspendUtc">The suspend time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier than current time.</exception>
        public Expiry Deactivate(DateTime? suspensionUtc = null)
        {
            // Keep idempotent if resubmitting suspending requests.
            if (this.suspensionUtc.HasValue)
            {
                return this;
            }

            var nowUtc = this.NowUtc;
            var finalSuspensionUtc = suspensionUtc ??= nowUtc;
            if (finalSuspensionUtc > nowUtc)
            {
                throw new ArgumentOutOfRangeException(nameof(suspensionUtc), "The suspension time must be earlier than current time.");
            }

            this.suspensionUtc = finalSuspensionUtc < this.DeadlineUtc ? finalSuspensionUtc : this.DeadlineUtc;
            return this;
        }

        /// <summary>
        /// Resume the expiry time.
        /// </summary>
        /// <param name="resumptionUtc">The resume time in UTC.</param>
        /// <param name="extended">Boolean flag to extend the expiry time after resumption.</param>
        /// <returns>The expiry time after resumption.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Resume time must be greater than suspend time.</exception>
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
