// <copyright file="CompositeBudget.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using System.Collections.Generic;

/// <summary>
/// Represents a composite budget strategy.
/// </summary>
public partial class CompositeBudget
{
    private readonly List<Budget> budgets = new ();

    /// <summary>
    /// Bind a budget strategy to the composite budget.
    /// </summary>
    /// <param name="budget">The budget strategy to bind.</param>
    /// <returns>The composite budget instance.</returns>
    public CompositeBudget Bind(Budget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        this.budgets.Add(budget);
        return this;
    }
}
