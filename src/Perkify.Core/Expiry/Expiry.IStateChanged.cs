// <copyright file="Expiry.IStateChanged.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using ExpiryStateChangeEventArgs = StateChangeEventArgs<ExpiryState, ExpiryStateOperation>;
    using ExpiryStateChangeExecutor = StateChangeExecutor<ExpiryState, ExpiryStateOperation, Expiry>;

    /// <inheritdoc/>
    public partial class Expiry : IStateChanged<ExpiryState, ExpiryStateOperation>
    {
        /// <inheritdoc/>
        public event EventHandler<ExpiryStateChangeEventArgs>? StateChanged;

        /// <summary>
        /// Gets the state change executor for the expiry state.
        /// </summary>
        public ExpiryStateChangeExecutor StateChangeExecutor => new (this, this.StateChanged)
        {
            StateRecorder = () => new ExpiryState(this.ExpiryUtc)
            {
                GracePeriod = this.GracePeriod,
            },
        };
    }
}
