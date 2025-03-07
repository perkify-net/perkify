// <copyright file="Budget.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Budget : IEligible
{
    /// <inheritdoc/>
    public bool IsEligible => !this.IsPaused;
}
