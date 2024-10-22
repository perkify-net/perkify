// <copyright file="StateChangeExecutor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Provides methods to execute actions and raise state change events.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TOperation">The type of the operation.</typeparam>
    /// <typeparam name="T">The type of the sender.</typeparam>
    public class StateChangeExecutor<TState, TOperation, T>(T sender, EventHandler<StateChangeEventArgs<TState, TOperation>>? handler)
        where T : IStateChanged<TState, TOperation>
    {
        /// <summary>
        /// Gets the event handler for state change events.
        /// </summary>
        public EventHandler<StateChangeEventArgs<TState, TOperation>>? EventHandler { get; } = handler;

        /// <summary>
        /// Gets the sender of the state change events.
        /// </summary>
        public T Sender { get; } = sender;

        /// <summary>
        /// Gets the function that records the current state.
        /// </summary>
        required public Func<TState> StateRecorder { get; init; }

        /// <summary>
        /// Executes the specified action and raises the state change event.
        /// </summary>
        /// <param name="operation">The operation to be performed on the state.</param>
        /// <param name="action">The action to execute.</param>
        public void Execute(TOperation operation, Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (this.EventHandler == null)
            {
                action();
                return;
            }

            var from = this.StateRecorder();
            action();
            var to = this.StateRecorder();
            this.EventHandler.Invoke(this.Sender, new StateChangeEventArgs<TState, TOperation>(operation)
            {
                From = from,
                To = to,
            });
        }
    }
}