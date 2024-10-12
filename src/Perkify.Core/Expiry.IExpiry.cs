// <copyright file="Expiry.IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;

    /// <inheritdoc/>
    public partial class Expiry : IExpiry<Expiry>
    {
        private DateTime? suspensionUtc;

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

        /// <inheritdoc/>
        public DateTime ExpiryUtc { get; private set; }

        /// <inheritdoc/>
        public Renewal? Renewal { get; private set; }

        /// <inheritdoc/>
        public DateTime? SuspensionUtc => this.suspensionUtc ?? (this.IsEligible ? null : this.GetDeadlineUtc());

        /// <inheritdoc/>
        public bool IsActive => !this.suspensionUtc.HasValue;

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
    }
}
