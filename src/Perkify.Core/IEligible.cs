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
}
