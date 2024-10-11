// <copyright file="Delegation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The delegation class to check the eligibility.</summary>
    /// <remarks>Create a delegation for eligibility.</remarks>
    /// <param name="fn">The function for eligibility check.</param>
    public class Delegation(Func<bool> fn)
        : IEligible
    {
        #region Implement IEligible interface

        /// <summary>Gets a value indicating whether see also in IEligible interface.</summary>
        public bool IsEligible => fn!();

        #endregion
    }
}
