// <copyright file="CompositeBudget.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class CompositeBudget : IEligible
{
    /// <inheritdoc/>
    public bool IsEligible => this.budgets.All(s => s.IsEligible);
}
