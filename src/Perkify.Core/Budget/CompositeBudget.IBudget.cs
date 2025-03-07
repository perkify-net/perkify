// <copyright file="CompositeBudget.IBudget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using System.Linq;

/// <inheritdoc/>
public partial class CompositeBudget : IBudget
{
    /// <inheritdoc/>
    public bool IsPaused => budgets.Count > 0 ? this.budgets.Any(s => s.IsPaused) : false;

    /// <inheritdoc/>
    public long Remaining => budgets.Count > 0 ? this.budgets.Min(s => s.Remaining) : 0L;

    /// <inheritdoc/>
    public long Verify(DateTime eventUtc, long amount, bool precheck = false)
    {
        // Skip verification if no budget strategy is binded.
        if (this.budgets.Count <= 0)
        {
            return amount;
        }

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

        // Find the minimum allowed deduction amount for budget reduction, throw BudgetExceededException directly if having
        var deduction = this.budgets.Select(b => b.Verify(eventUtc, amount, precheck: true)).Min();

        // Run budget deduction on all budget strategies if precheck is true
        if (!precheck)
        {
            this.budgets.ForEach(b => b.Verify(eventUtc, deduction, precheck));
        }

        return deduction;
    }
}
