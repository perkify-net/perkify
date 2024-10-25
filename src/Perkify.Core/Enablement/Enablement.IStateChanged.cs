// <copyright file="Enablement.IStateChanged.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using EnablementStateChangeEventArgs = StateChangeEventArgs<EnablementState, EnablementStateOperation>;
using EnablementStateChangeExecutor = StateChangeExecutor<EnablementState, EnablementStateOperation, Enablement>;

/// <inheritdoc/>
public partial class Enablement : IStateChanged<EnablementState, EnablementStateOperation>
{
    /// <inheritdoc/>
    public event EventHandler<EnablementStateChangeEventArgs>? StateChanged;

    /// <summary>
    /// Gets the state change executor for the enablement state.
    /// </summary>
    public EnablementStateChangeExecutor StateChangeExecutor => new (this, this.StateChanged)
    {
        StateRecorder = () => new EnablementState(this.IsActive)
        {
            EffectiveUtc = this.EffectiveUtc,
            IsImmediateEffective = this.IsImmediateEffective,
        },
    };
}