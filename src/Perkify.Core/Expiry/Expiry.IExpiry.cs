// <copyright file="Expiry.IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Expiry : IExpiry
    {
        /// <inheritdoc/>
        public DateTime ExpiryUtc { get; private set; } = expiryUtc;

        /// <inheritdoc/>
        public TimeSpan GracePeriod { get; set; } = TimeSpan.Zero;

        /// <inheritdoc/>
        public DateTime DeadlineUtc => this.ExpiryUtc + this.GracePeriod;

        /// <inheritdoc/>
        public bool IsExpired => this.Clock.GetCurrentInstant().ToDateTimeUtc() >= this.ExpiryUtc;

        /// <inheritdoc/>
        public TimeSpan Overdue => this.Clock.GetCurrentInstant().ToDateTimeUtc() switch
        {
            var nowUtc when nowUtc <= this.ExpiryUtc => TimeSpan.Zero,
            var nowUtc when nowUtc < this.DeadlineUtc => nowUtc - this.ExpiryUtc,
            var _ => this.GracePeriod
        };

        /// <inheritdoc/>
        public ChronoInterval? Renewal { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Remaining(bool deadline = false)
        {
            var origin = deadline ? this.DeadlineUtc : this.ExpiryUtc;
            var delta = origin - this.Clock.GetCurrentInstant().ToDateTimeUtc();
            return delta >= TimeSpan.Zero ? delta : TimeSpan.Zero;
        }

        /// <inheritdoc/>
        public void Renew(string? interval = null)
        {
            var renewal = interval != null
                ? new ChronoInterval(interval)
                : this.Renewal ?? throw new ArgumentNullException(nameof(interval), "Renewal interval is required.");

            var previousExpiryUtc = this.ExpiryUtc;
            var nextExpiryUtc = renewal.Renew(previousExpiryUtc);
            if (nextExpiryUtc <= previousExpiryUtc)
            {
                throw new InvalidOperationException("Negative ISO8601 duration.");
            }

            this.ExpiryUtc = nextExpiryUtc;
            if (interval != null)
            {
                this.Renewal = renewal;
            }
        }

        /// <inheritdoc/>
        public void AdjustTo(DateTime expiryUtc)
            => this.ExpiryUtc = expiryUtc;
    }
}
