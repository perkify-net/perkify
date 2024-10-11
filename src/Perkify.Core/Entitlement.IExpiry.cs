// <copyright file="Entitlement.IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : INowUtc, IExpiry<Entitlement>
    {
        /// <inheritdoc/>
        public DateTime NowUtc => this.expiry?.NowUtc ?? DateTime.UtcNow;

        /// <inheritdoc/>
        public TimeSpan GracePeriod => this.expiry?.GracePeriod ?? TimeSpan.Zero;

        /// <inheritdoc/>
        public TimeSpan Remaining => this.expiry?.Remaining ?? TimeSpan.MaxValue;

        /// <inheritdoc/>
        public TimeSpan Overdue => this.expiry?.Overdue ?? TimeSpan.Zero;

        /// <inheritdoc/>
        public DateTime ExpiryUtc => this.expiry?.ExpiryUtc ?? DateTime.MaxValue;

        /// <inheritdoc/>
        public Renewal? Renewal => this.expiry?.Renewal;

        /// <inheritdoc/>
        public DateTime? SuspensionUtc => this.expiry?.SuspensionUtc;

        /// <inheritdoc/>
        public bool IsActive => this.expiry?.IsActive ?? true;

        /// <inheritdoc/>
        public Entitlement Renew(Renewal? renewal)
        {
            this.expiry?.Renew(renewal);
            return this;
        }

        /// <inheritdoc/>
        public Entitlement Deactivate(DateTime? suspensionUtc = null)
        {
            this.expiry?.Deactivate(suspensionUtc);
            return this;
        }

        /// <inheritdoc/>
        public Entitlement Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            this.expiry?.Activate(resumptionUtc, extended);
            return this;
        }
    }
}
