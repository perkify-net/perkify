// <copyright file="IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The interface for activation and deactivate operations.
    /// </summary>
    public interface IEnablement
    {
        /// <summary>
        /// Gets a value indicating whether the state is active.
        /// - true if the state is active (activated or not suspended).
        /// - false if the state is inactive (deactivated or suspended).
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// Gets the deactivation time in UTC.
        /// - null if the state is active.
        /// - not null if the state is inactive.
        /// </summary>
        public DateTime? DeactivationUtc { get; }

        /// <summary>
        /// Activate and switch to active state.
        /// </summary>
        /// <param name="activationUtc">The activation time in UTC.</param>
        public void Activate(DateTime? activationUtc = null);

        /// <summary>
        /// Deactivate and switch to inactive state.
        /// </summary>
        /// <param name="deactivationUtc">The deactivation time in UTC.</param>
        public void Deactivate(DateTime? deactivationUtc = null);
    }
}
