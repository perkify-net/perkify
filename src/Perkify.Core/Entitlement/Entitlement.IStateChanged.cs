// <copyright file="Entitlement.IStateChanged.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using BalanceStateChangeEventArgs = StateChangeEventArgs<BalanceState, BalanceStateOperation>;
using EnablementStateChangeEventArgs = StateChangeEventArgs<EnablementState, EnablementStateOperation>;
using ExpiryStateChangeEventArgs = StateChangeEventArgs<ExpiryState, ExpiryStateOperation>;

/// <summary>
/// Represents the entitlement class with state change events.
/// </summary>
public partial class Entitlement
{
    /// <summary>
    /// Occurs when the enablement state changes.
    /// </summary>
    public event EventHandler<EnablementStateChangeEventArgs>? EnablementStateChanged;

    /// <summary>
    /// Occurs when the expiry state changes.
    /// </summary>
    public event EventHandler<ExpiryStateChangeEventArgs>? ExpiryStateChanged;

    /// <summary>
    /// Occurs when the balance state changes.
    /// </summary>
    public event EventHandler<BalanceStateChangeEventArgs>? BalanceStateChanged;
}
