// <copyright file="Budget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// To control the budget for all incoming/outgoing events.
/// </summary>
/// <param name="upperLimit">The upper limit of the budget.</param>
/// <param name="usage">The current usage amount.</param>
/// <param name="nextResetUtc">The next UTC reset time for budget smoothing.</param>
public partial class Budget(long upperLimit, long usage = 0, DateTime nextResetUtc = default)
    : IEligible
{
    /// <summary>
    /// Gets the policy for handling budget exceedance.
    /// </summary>
    public BalanceExceedancePolicy Policy { get; init; } = BalanceExceedancePolicy.Reject;

    /// <summary>
    /// Gets the upper limit of the budget.
    /// </summary>
    public long UpperLimit { get; init; } = upperLimit;

    /// <summary>
    /// Gets the smoothing interval for budget reset.
    /// </summary>
    public TimeSpan SmoothInterval { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Gets the next UTC reset time for budget smoothing.
    /// </summary>
    public DateTime NextResetUtc { get; private set; } = nextResetUtc;

    /// <summary>
    /// Gets the current usage amount.
    /// </summary>
    public long Usage { get; private set; } = usage;
}
