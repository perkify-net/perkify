// <copyright file="CompositeBudget.IBudget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using System.Linq;

/// <inheritdoc/>
public partial class CompositeBudget : IBudget
{
    /// <inheritdoc/>
    public bool IsPaused => this.budgets.Any(s => s.IsPaused);

    /// <inheritdoc/>
    public long Remaining => this.budgets.Min(s => s.Remaining);

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
