// <copyright file="Enablement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Enablement : IEnablement
    {
        /// <inheritdoc/>
        public bool IsActive => !this.DeactivationUtc.HasValue;

        /// <inheritdoc/>
        public DateTime? DeactivationUtc { get; private set; } = deactivationUtc;

        /// <inheritdoc/>
        public void Activate(DateTime? activationUtc)
        {
            if (this.IsActive)
            {
                throw new InvalidOperationException("Already in active state.");
            }

            this.DeactivationUtc = null;
        }

        /// <inheritdoc/>
        public void Deactivate(DateTime? deactivationUtc)
        {
            if (!this.IsActive)
            {
                throw new InvalidOperationException("Already in inactive state.");
            }

            deactivationUtc ??= this.NowUtc;
            this.DeactivationUtc = deactivationUtc;
        }
    }
}
