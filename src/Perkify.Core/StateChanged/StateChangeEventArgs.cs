// <copyright file="StateChangeEventArgs.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Provides data for the state change event.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TOperation">The type of the operation.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="StateChangeEventArgs{TState, TOperation}"/> class.
    /// </remarks>
    /// <param name="operation">The operation to be performed on the state.</param>
    public class StateChangeEventArgs<TState, TOperation>(TOperation operation)
        : EventArgs
    {
        /// <summary>
        /// Gets the operation to be performed on the state.
        /// </summary>
        public TOperation Operation { get; } = operation;

        /// <summary>
        /// Gets the previous state.
        /// </summary>
        required public TState From { get; init; }

        /// <summary>
        /// Gets the new state.
        /// </summary>
        required public TState To { get; init; }
    }
}