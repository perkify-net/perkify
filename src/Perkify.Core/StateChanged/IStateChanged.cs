// <copyright file="IStateChanged.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// Interface for handling state changes.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TOperation">The type of the operation.</typeparam>
public interface IStateChanged<TState, TOperation>
{
    /// <summary>
    /// Event triggered when the state changes.
    /// </summary>
    public event EventHandler<StateChangeEventArgs<TState, TOperation>>? StateChanged;
}