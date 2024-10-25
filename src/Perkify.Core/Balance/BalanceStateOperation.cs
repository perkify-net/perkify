// <copyright file="BalanceStateOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// Represents the operations that can be performed on a balance state.
/// </summary>
public enum BalanceStateOperation
{
    /// <summary>
    /// Represents a top-up operation.
    /// </summary>
    Topup,

    /// <summary>
    /// Represents a deduction operation.
    /// </summary>
    Deduct,

    /// <summary>
    /// Represents an adjustment operation.
    /// </summary>
    Adjust,
}
