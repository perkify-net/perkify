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
        public TimeSpan GracePeriod { get; private set; } = grace ?? TimeSpan.Zero;

        /// <inheritdoc/>
        public DateTime DeadlineUtc => this.ExpiryUtc + this.GracePeriod;

        /// <inheritdoc/>
        public bool IsExpired => this.NowUtc >= this.ExpiryUtc;

        /// <inheritdoc/>
        public TimeSpan Remaining
        {
            get
            {
                var delta = this.DeadlineUtc - this.NowUtc;
                return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
            }
        }

        /// <inheritdoc/>
        public TimeSpan Overdue => this.NowUtc switch
        {
            var nowUtc when nowUtc <= this.ExpiryUtc => TimeSpan.Zero,
            var nowUtc when nowUtc < this.DeadlineUtc => nowUtc - this.ExpiryUtc,
            var _ => this.GracePeriod
        };

        /// <inheritdoc/>
        public ChronoInterval? Renewal { get; private set; }

        /// <inheritdoc/>
        public void Renew(ChronoInterval? renewal = null)
        {
            renewal ??= this.Renewal;
            if (renewal == null)
            {
                throw new ArgumentNullException(nameof(renewal), "Renewal period is required.");
            }

            var previousExpiryUtc = this.ExpiryUtc;
            var nextExpiryUtc = renewal.Renew(previousExpiryUtc);
            if (nextExpiryUtc <= previousExpiryUtc)
            {
                throw new InvalidOperationException("Negative ISO8601 duration.");
            }

            this.ExpiryUtc = nextExpiryUtc;
            this.Renewal = renewal;
        }
    }
}
