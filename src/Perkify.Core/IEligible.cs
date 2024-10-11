// <copyright file="IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The eligibility interface.</summary>
    public interface IEligible
    {
        /// <summary>Gets a value indicating whether the eligibility status.</summary>
        bool IsEligible { get; }
    }

    /// <summary>The extension methods for the eligibility.</summary>
    public static class IEligibleExtensions
    {
        /// <summary>Check if the eligibility is valid.</summary>
        /// <param name="eligible">The eligibility to check.</param>
        /// <exception cref="InvalidOperationException">Thrown when the eligibility is invalid.</exception>
        public static void Check<T>(this T eligible)
            where T : IEligible
        {
            if (!eligible.IsEligible)
            {
                throw new InvalidOperationException("The eligibility is invalid.");
            }
        }
    }

    /// <summary>The delegation class to check the eligibility.</summary>
    /// <remarks>Create a delegation for eligibility.</remarks>
    /// <param name="fn">The function for eligibility check.</param>
    public class Delegation(Func<bool> fn): IEligible
    {
        #region Implement IEligible interface

        /// <summary>Gets a value indicating whether see also in IEligible interface.</summary>
        public bool IsEligible => fn();

        #endregion
    }
}
