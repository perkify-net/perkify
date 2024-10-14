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
        public TimeSpan Overdue => this.Expiry!.Overdue;

        /// <inheritdoc/>
        public ChronoInterval? Renewal => this.Expiry!.Renewal;

        /// <inheritdoc/>
        public TimeSpan Remaining(bool deadline = false) => this.Expiry!.Remaining(deadline);

        /// <inheritdoc/>
        public void Renew(string? interval)
            => this.Expiry!.Renew(interval);
    }
}
