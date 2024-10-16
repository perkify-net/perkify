// <copyright file="Entitlement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IEnablement
    {
        /// <inheritdoc/>
        public event EventHandler<EnablementStateChangeEventArgs>? EnablementStateChanged;

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
        public void Deactivate(DateTime? effectiveUtc = null, bool isImmediateEffective = true)
            => this.enablement!.Deactivate(effectiveUtc, isImmediateEffective);

        /// <inheritdoc/>
        public void Activate(DateTime? effectiveUtc = null, bool isImmediateEffective = true)
        {
            // Deduct overdue time from activation time if need.
            var overdue = TimeSpan.Zero;
            if (this.expiry != null && this.AutoRenewalMode.HasFlag(AutoRenewalMode.Enablement))
            {
                var deactivationUtc = this.EffectiveUtc;
                overdue = deactivationUtc - this.ExpiryUtc;
                overdue = overdue > TimeSpan.Zero ? overdue : TimeSpan.Zero;
                overdue = overdue < this.Overdue ? overdue : this.Overdue;
            }

            effectiveUtc = effectiveUtc ?? this.NowUtc;
            effectiveUtc -= overdue;
            this.enablement!.Activate(effectiveUtc, isImmediateEffective);
        }
    }
}
