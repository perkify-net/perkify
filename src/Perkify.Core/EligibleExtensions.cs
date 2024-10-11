// <copyright file="EligibleExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The extension methods for the eligibility.</summary>
    public static class EligibleExtensions
    {
        /// <summary>Check if the eligibility is valid.</summary>
        /// <typeparam name="T">The type of the eligibility to check.</typeparam>
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
}
