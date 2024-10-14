﻿// <copyright file="Enablement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Enablement : IEnablement
    {
        /// <inheritdoc/>
        public bool IsActive { get; }

        /// <inheritdoc/>
        public DateTime? DeactivationUtc { get; private set; }

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
            deactivationUtc ??= this.NowUtc;

            if (this.IsActive)
            {
                this.DeactivationUtc = deactivationUtc;
                return;
            }

            // Ignore earlier deactivation
            if (deactivationUtc >= this.DeactivationUtc!.Value)
            {
                throw new InvalidOperationException("Already in inactive state.");
            }
        }
    }
}
