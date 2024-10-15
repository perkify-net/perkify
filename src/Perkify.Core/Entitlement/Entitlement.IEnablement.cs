// <copyright file="Entitlement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IEnablement
    {
        /// <inheritdoc/>
        public event EventHandler<EnablementStateChangeEventArgs>? StateChanged;

        /// <inheritdoc/>
        public bool IsActive => this.Enablement!.IsActive;

        /// <inheritdoc/>
        public DateTime EffectiveUtc => this.Enablement!.EffectiveUtc;

        /// <inheritdoc/>
        public bool IsImmediateEffective => this.Enablement!.IsImmediateEffective;

        /// <inheritdoc/>
        public void Deactivate(DateTime? effectiveUtc = null, bool isImmediateEffective = true)
            => this.Enablement!.Deactivate(effectiveUtc, isImmediateEffective);

        /// <inheritdoc/>
        public void Activate(DateTime? effectiveUtc = null, bool isImmediateEffective = true)
            => this.Enablement!.Activate(effectiveUtc, isImmediateEffective);
    }
}
