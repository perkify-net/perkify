// <copyright file="Entitlement.IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IExpiry
    {
        /// <inheritdoc/>
        public DateTime ExpiryUtc
            => this.expiry!.ExpiryUtc;

        /// <inheritdoc/>
        public TimeSpan GracePeriod
        {
            get => this.expiry!.GracePeriod;
            set => this.expiry!.GracePeriod = value;
        }

        /// <inheritdoc/>
        public DateTime DeadlineUtc
            => this.expiry!.DeadlineUtc;

        /// <inheritdoc/>
        public bool IsExpired
            => this.expiry!.IsExpired;

        /// <inheritdoc/>
        public TimeSpan Overdue
            => this.expiry!.Overdue;

        /// <inheritdoc/>
        public ChronoInterval? Renewal
            => this.expiry!.Renewal;

        /// <inheritdoc/>
        public TimeSpan Remaining(bool deadline = false)
            => this.expiry!.Remaining(deadline);

        /// <inheritdoc/>
        public void Renew(string? interval)
            => this.expiry!.Renew(interval);

        /// <inheritdoc/>
        public void AdjustTo(DateTime expiryUtc)
            => this.expiry!.AdjustTo(expiryUtc);
    }
}
