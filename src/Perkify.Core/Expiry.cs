namespace Perkify.Core
{
    using NodaTime;

    /// <summary>Expiry time for eligibility.</summary>
    public class Expiry : INowUtc, IEligible, IExpiry<Expiry>
    {
        /// <summary>Create the expiry time for eligibility.</summary>
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

        public DateTime NowUtc => this.clock.GetCurrentInstant().ToDateTimeUtc();

        #endregion

        #region Implements IEligible interface

        /// <summary>
        /// See also in IEligible interface.
        /// </summary>
        public bool IsEligible => !this.suspensionUtc.HasValue && this.NowUtc < this.GetDeadlineUtc();

        #endregion

        #region Implements IExpiry<T> interface

        #region IsExpired, Remaining & Overdue

        /// <summary>
        /// Grace period.
        /// </summary>
        public TimeSpan GracePeriod { get; private set; }

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
                    return nowUtc <= this.GetDeadlineUtc() ? this.ExpiryUtc - nowUtc : this.GracePeriod.Negate();
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
                var nowUtc when nowUtc < this.GetDeadlineUtc() => nowUtc - this.ExpiryUtc,
                var _ => this.GracePeriod
            };

        #endregion

        #region Renew

        /// <summary>The expiry time in UTC.</summary>
        public DateTime ExpiryUtc { get; private set; }

        /// <summary>The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
        public Renewal? Renewal { get; private set; }

        /// <summary>
        /// Renew the expiry time in timeline arithmetic or calendrical arithmetic.
        /// </summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        /// <returns>The expiry time after renewal.</returns>
        public Expiry Renew(Renewal? renewal = null)
        {
            renewal ??= this.Renewal;
            if(renewal == null)
            {
                throw new ArgumentNullException(nameof(renewal), "Renewal period is required.");
            }

            if (!this.IsActive)
            {
                throw new InvalidOperationException("Suspended state.");
            }

            var previousExpiryUtc = this.ExpiryUtc;
            var nextExpiryUtc = renewal.Renew(previousExpiryUtc);
            if(nextExpiryUtc <= previousExpiryUtc)
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

        /// <summary>
        /// The suspend time in UTC.
        /// - Explicit Suspension Time (Suspend)
        /// - Implicit Suspension Time (Deadline) when the expiry time is not eligible.
        /// - Null when the expiry time is eligible.
        /// </summary>
        public DateTime? SuspensionUtc => this.suspensionUtc ?? (this.IsEligible ? null : this.GetDeadlineUtc());

        /// <summary>
        /// Boolean flag to identify if the expiry time is suspended.
        /// </summary>
        public bool IsActive => !this.suspensionUtc.HasValue;

        /// <summary>Suspends the expiry time.</summary>
        /// <param name="suspensionUtc">The suspend time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier than current time.</exception>
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

        /// <summary>Resume the expiry time.</summary>
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
