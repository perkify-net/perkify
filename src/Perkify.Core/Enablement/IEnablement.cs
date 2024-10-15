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
        /// Occurs when the state changes.
        /// </summary>
        public event EventHandler<EnablementStateChangeEventArgs>? StateChanged;

        /// <summary>
        /// Gets a value indicating whether the state is active.
        /// - true if the state is active (activated or not suspended).
        /// - false if the state is inactive (deactivated or suspended).
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// Gets the effective UTC time for the state change.
        /// </summary>
        public DateTime EffectiveUtc { get; }

        /// <summary>
        /// Gets a value indicating whether the state change is immediately effective.
        /// - true if the state change is immediately effective, aligned to the current IsActive state.
        /// - false if the state change will take effect in future, reversed to the current IsActive state.
        /// </summary>
        public bool IsImmediateEffective { get; }

        /// <summary>
        /// Activate and switch to active state.
        /// </summary>
        /// <param name="effectiveUtc">The activation time in UTC.</param>
        /// <param name="isImmediateEffective">Indicates if the activation is immediate.</param>
        public void Activate(DateTime? effectiveUtc = null, bool isImmediateEffective = true);

        /// <summary>
        /// Deactivate and switch to inactive state.
        /// </summary>
        /// <param name="effectiveUtc">The deactivation time in UTC.</param>
        /// <param name="isImmediateEffective">Indicates if the deactivation is immediate.</param>
        public void Deactivate(DateTime? effectiveUtc = null, bool isImmediateEffective = true);
    }
}
