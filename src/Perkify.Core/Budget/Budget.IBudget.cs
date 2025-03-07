// <copyright file="Budget.IBudget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Budget : IBudget
{
    /// <inheritdoc/>
    public bool IsPaused => this.UpperLimit <= this.Usage;

    /// <inheritdoc/>
    public long Remaining => this.UpperLimit - this.Usage;

    /// <inheritdoc/>
    public long Verify(DateTime eventUtc, long amount, bool precheck = false)
    {
        // TODO: fix if-throw pattern
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        // TODO: fix if-throw pattern
        if (this.IsPaused)
        {
            throw new BudgetExceededException("Budget paused.");
        }

        // Reset usage for budget smooth when required
        if (!precheck && this.SmoothInterval != TimeSpan.Zero && eventUtc >= this.NextResetUtc)
        {
            this.NextResetUtc = eventUtc.Add(this.SmoothInterval);
            this.Usage = 0;
        }

        // Accumulate usage if budget limit not exceeded
        if (this.Remaining >= amount)
        {
            if (!precheck)
            {
                this.Usage += amount;
            }

            return amount;
        }

        // Handle budget exceeded
        switch (this.Policy)
        {
            case BalanceExceedancePolicy.Reject:
                throw new BudgetExceededException($"Budget exceeded: {this.UpperLimit}");

            case BalanceExceedancePolicy.Overflow:
                var available = this.Remaining;
                if (!precheck)
                {
                    this.Usage += available;
                }

                return available;

            case BalanceExceedancePolicy.Overdraft:
                if (!precheck)
                {
                    this.Usage += amount;
                }
                
                return amount;

            default:
                throw new InvalidOperationException($"Unsupported policy: {this.Policy}");
        }
    }
}
