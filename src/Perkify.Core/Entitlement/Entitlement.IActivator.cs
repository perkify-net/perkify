// <copyright file="Entitlement.IActivator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Entitlement : IEnablement
    {
        /// <inheritdoc/>
        public DateTime? DeactivationUtc => this.Enablement?.DeactivationUtc;

        /// <inheritdoc/>
        public bool IsActive => this.Enablement?.IsActive ?? true;

        /// <inheritdoc/>
        public void Deactivate(DateTime? deactivationUtc = null)
            => this.Enablement!.Deactivate(deactivationUtc);

        /// <inheritdoc/>
        public void Activate(DateTime? activationUtc = null)
        {
            if (this.Enablement == null)
            {
                throw new NotSupportedException("Activator is not specified.");
            }

            this.Enablement!.Activate(activationUtc);
        }
    }
}
