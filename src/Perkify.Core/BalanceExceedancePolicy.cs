// <copyright file="BalanceExceedancePolicy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The overflow processing policy when the balance is exceeded.</summary>
    public enum BalanceExceedancePolicy
    {
        /// <summary>Raise an exception when the balance is exceeded.</summary>
        Reject = 0,

        /// <summary>Deduct the balance from the threshold when the balance is exceeded.</summary>
        Overflow = 1,

        /// <summary>Allow the balance to be exceeded.</summary>
        Overdraft = 2,
    }
}
