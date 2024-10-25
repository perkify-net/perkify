// <copyright file="Balance.IStateChanged.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using BalanceStateChangeEventArgs = StateChangeEventArgs<BalanceState, BalanceStateOperation>;
using BalanceStateChangeExecutor = StateChangeExecutor<BalanceState, BalanceStateOperation, Balance>;

/// <inheritdoc/>
public partial class Balance : IStateChanged<BalanceState, BalanceStateOperation>
{
    /// <inheritdoc/>
    public event EventHandler<BalanceStateChangeEventArgs>? StateChanged;

    /// <summary>
    /// Gets the state change executor for the balance state.
    /// </summary>
    public BalanceStateChangeExecutor StateChangeExecutor => new (this, this.StateChanged)
    {
        StateRecorder = () => new BalanceState(this.Threshold, this.BalanceExceedancePolicy)
        {
            Incoming = this.Incoming,
            Outgoing = this.Outgoing,
        },
    };
}
