﻿// <copyright file="Expiry.IExpiry.cs" company="Microsoft">
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
        public TimeSpan? GracePeriod
        {
            get => grace;
            set => grace = value;
        }

        /// <inheritdoc/>
        public DateTime DeadlineUtc => this.ExpiryUtc + (this.GracePeriod ?? TimeSpan.Zero);

        /// <inheritdoc/>
        public bool IsExpired => this.NowUtc >= this.ExpiryUtc;

        /// <inheritdoc/>
        public TimeSpan Overdue => this.NowUtc switch
        {
            var nowUtc when nowUtc <= this.ExpiryUtc => TimeSpan.Zero,
            var nowUtc when nowUtc < this.DeadlineUtc => nowUtc - this.ExpiryUtc,
            var _ => this.GracePeriod ?? TimeSpan.Zero
        };

        /// <inheritdoc/>
        public ChronoInterval? Renewal { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Remaining(bool deadline = false)
        {
            var origin = deadline ? this.DeadlineUtc : this.ExpiryUtc;
            var delta = origin - this.NowUtc;
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
