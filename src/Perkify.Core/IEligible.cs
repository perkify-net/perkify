// <copyright file="IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// The eligibility interface.
/// </summary>
public interface IEligible
{
    /// <summary>
    /// Gets a value indicating whether the eligibility status.
    /// </summary>
    public bool IsEligible { get; }

    /// <summary>
    /// Check if the eligibility is valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the eligibility is invalid.</exception>
    public void Check()
    {
        if (!this.IsEligible)
        {
            throw new InvalidOperationException("Ineligible state.");
        }
    }
}
