// <copyright file="Entitlement.IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IExpiry
    {
        /// <inheritdoc/>
        public DateTime ExpiryUtc => this.Expiry!.ExpiryUtc;

        /// <inheritdoc/>
        public TimeSpan GracePeriod => this.Expiry!.GracePeriod;

        /// <inheritdoc/>
        public DateTime DeadlineUtc => this.Expiry!.DeadlineUtc;

        /// <inheritdoc/>
        public bool IsExpired => this.Expiry!.IsExpired;

        /// <inheritdoc/>
        public TimeSpan Remaining => this.Expiry!.Remaining;

        /// <inheritdoc/>
        public TimeSpan Overdue => this.Expiry!.Overdue;

        /// <inheritdoc/>
        public ChronoInterval? Renewal => this.Expiry!.Renewal;

        /// <inheritdoc/>
        public void Renew(ChronoInterval? renewal)
            => this.Expiry!.Renew(renewal);
    }
}
