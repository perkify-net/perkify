// <copyright file="EnablementStateChangeEventArgs.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Provides data for the enablement state change event.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EnablementStateChangeEventArgs"/> class.
    /// </remarks>
    /// <param name="operation">The operation to be performed on the enablement state.</param>
    public class EnablementStateChangeEventArgs(EnablemenStateOperation operation)
        : EventArgs
    {
        /// <summary>
        /// Gets the operation to be performed on the enablement state.
        /// </summary>
        public EnablemenStateOperation Operation { get; } = operation;

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
