// <copyright file="Budget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// To control the budget for all incoming/outgoing events.
/// </summary>
public class Budget(long upperLimit, long usage = 0, DateTime nextResetUtc = default)
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

    /// <inheritdoc/>
    public bool IsEligible => this.Usage <= this.UpperLimit;

    /// <summary>
    /// Verify the budget usage and return the amount to be deducted.
    /// </summary>
    /// <param name="eventUtc">Event timestamp in UTC.</param>
    /// <param name="amount">Amount to deduct.</param>
    /// <returns>Actual deductible amount.</returns>
    public decimal VerifyBudgetUsage(DateTime eventUtc, long amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative");
        }

        // Reset usage for budget smooth when required
        if (this.SmoothInterval != TimeSpan.Zero && eventUtc >= this.NextResetUtc)
        {
            this.NextResetUtc = eventUtc.Add(this.SmoothInterval);
            this.Usage = 0;
        }

        // Accumulate usage if budget limit not exceeded
        var available = this.UpperLimit - this.Usage;
        if (available >= amount)
        {
            this.Usage += amount;
            return amount;
        }

        // Handle budget exceeded
        switch (this.Policy)
        {
            case BalanceExceedancePolicy.Reject:
                throw new BudgetExceededException($"Budget exceeded: {this.UpperLimit}");

            case BalanceExceedancePolicy.Overflow:
                this.Usage += available;
                return available;

            case BalanceExceedancePolicy.Overdraft:
                if (this.Usage >= this.UpperLimit)
                {
                    throw new BudgetExceededException($"Budget exceeded: {this.UpperLimit}");
                }

                this.Usage += amount;
                return amount;

            default:
                throw new InvalidOperationException($"Unsupported policy: {this.Policy}");
        }
    }
}
