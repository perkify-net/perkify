// <copyright file="Entitlement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IEnablement
    {
        /// <inheritdoc/>
        public bool IsActive
            => this.enablement!.IsActive;

        /// <inheritdoc/>
        public DateTime EffectiveUtc
            => this.enablement!.EffectiveUtc;

        /// <inheritdoc/>
        public bool IsImmediateEffective
            => this.enablement!.IsImmediateEffective;

        /// <inheritdoc/>
        public void Deactivate(DateTime? effectiveUtc = null)
            => this.enablement!.Deactivate(effectiveUtc);

        /// <inheritdoc/>
        public void Activate(DateTime? effectiveUtc = null)
        {
            // Deduct overdue time from activation time if need.
            var overdue = TimeSpan.Zero;
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Enablement))
            {
                var deactivationUtc = this.EffectiveUtc;
                var overdueOnDeactivation = deactivationUtc - this.ExpiryUtc;
                var overdueOnExpiry = this.expiry.Overdue;
                overdue = overdueOnDeactivation < overdueOnExpiry ? overdueOnDeactivation : overdueOnExpiry;
            }

            this.enablement!.Activate(effectiveUtc);
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Enablement))
            {
                var activationUtc = this.enablement.EffectiveUtc;
                var newExpiryUtc = activationUtc - overdue;
                this.expiry.AdjustTo(newExpiryUtc);
            }
        }
    }
}
