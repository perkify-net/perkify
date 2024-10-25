// <copyright file="BalanceExceedancePolicy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// The overflow processing policy when the balance is exceeded.
/// </summary>
public enum BalanceExceedancePolicy
{
    /// <summary>
    /// Raise an exception when the balance is exceeded.
    /// NOTE:
    /// 1. Expcetion will be raised when the balance will be exceeded after deduction.
    /// 2. The outgoing amount will not be changed if the deduction is rejected.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// Deduct the balance from the threshold when the balance is exceeded.
    /// NOTE:
    /// 1. It is allowed to process deduction partially to reach its upper limit.
    /// 2. The remained amount of unprocessed deduction will be returned for further processing.
    /// </summary>
    Overflow = 1,

    /// <summary>
    /// Allow the balance to be exceeded.
    /// NOTE:
    /// 1. Before balance deduction with overdraft policy, the balance should be eligible.
    /// 2. After balance deduction with overdraft policy, the balance is no longer eligible
    /// 3. And the ineligible balance can't be deducted anymore.
    /// So, the balance deduction with overdrafting can processed only once until the balance is eligible again.
    /// </summary>
    Overdraft = 2,
}
