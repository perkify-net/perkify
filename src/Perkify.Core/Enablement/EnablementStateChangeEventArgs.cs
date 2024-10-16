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
        /// Gets the previous enablement state.
        /// </summary>
        required public EnablementState From { get; init; }

        /// <summary>
        /// Gets the new enablement state.
        /// </summary>
        required public EnablementState To { get; init; }
    }
}
