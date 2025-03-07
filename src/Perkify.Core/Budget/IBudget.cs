// <copyright file="IBudget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// To control the budget for incoming or outgoing.
/// </summary>
public interface IBudget
{
    /// <summary>
    /// Gets a value indicating whether the budget is paused.
    /// </summary>
    public bool IsPaused { get; }

    /// <summary>
    /// Gets the remaining budget amount.
    /// </summary>
    public long Remaining { get; }

    /// <summary>
    /// Verify the budget usage and return the amount to be deducted.
    /// </summary>
    /// <param name="eventUtc">Event timestamp in UTC.</param>
    /// <param name="amount">Amount to deduct.</param>
    /// <param name="precheck">Whether to perform a precheck without deducting the amount.</param>
    /// <returns>Actual deductible amount.</returns>
    public long Verify(DateTime eventUtc, long amount, bool precheck = false);
}
