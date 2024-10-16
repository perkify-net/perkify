// <copyright file="EnablementState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents the state of enablement.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EnablementState"/> class.
    /// </remarks>
    /// <param name="isActive">A value indicating whether the enablement state is active.</param>
    public class EnablementState(bool isActive)
    {
        /// <summary>
        /// Gets a value indicating whether the enablement state is active.
        /// </summary>
        public bool IsActive { get; } = isActive;

        /// <summary>
        /// Gets the effective UTC date and time of the operation.
        /// </summary>
        public DateTime EffictiveUtc { get; init; }

        /// <summary>
        /// Gets a value indicating whether the operation is effective immediately.
        /// </summary>
        public bool IsImmediateEffective { get; init; }
    }
}
