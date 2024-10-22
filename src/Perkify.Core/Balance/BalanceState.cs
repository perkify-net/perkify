// <copyright file="BalanceState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents the state of a balance with a threshold and a policy for handling exceedances.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BalanceState"/> class.
    /// </remarks>
    /// <param name="threshold">The threshold value for the balance.</param>
    /// <param name="policy">The policy to apply when the balance exceeds the threshold.</param>
    public class BalanceState(long threshold, BalanceExceedancePolicy policy)
    {
        /// <summary>
        /// Gets the threshold value for the balance.
        /// </summary>
        public long Threshold { get; } = threshold;

        /// <summary>
        /// Gets the policy to apply when the balance exceeds the threshold.
        /// </summary>
        public BalanceExceedancePolicy BalanceExceedancePolicy { get; } = policy;

        /// <summary>
        /// Gets the incoming balance amount.
        /// </summary>
        public long Incoming { get; init; }

        /// <summary>
        /// Gets the outgoing balance amount.
        /// </summary>
        public long Outgoing { get; init; }
    }
}
