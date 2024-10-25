// <copyright file="BalanceType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>The balance type: debit or credit.</summary>
public enum BalanceType
{
    /// <summary>
    /// Debit points.
    /// - Threshold is zero.
    /// - Amount should beyond Threshold.
    /// </summary>
    Debit = 0,

    /// <summary>
    /// Credit points.
    /// - Threshold is negative.
    /// - Amount should beyond Threshold.
    /// </summary>
    Credit = 1,
}
