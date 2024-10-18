// <copyright file="IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The interface to maintain a balance amount with threshold for eligibility.
    /// </summary>
    public interface IBalance
    {
        /// <summary>
        /// Gets the threshold amount for the balance.
        /// </summary>
        public long Threshold { get; }

        /// <summary>
        /// Gets the balance exceedance policy.
        /// </summary>
        public BalanceExceedancePolicy BalanceExceedancePolicy { get; }

        /// <summary>
        /// Gets the balance type: debit or credit.
        /// </summary>
        public BalanceType BalanceType { get; }

        /// <summary>
        /// Gets all incoming revenue to the balance.
        /// </summary>
        public long Incoming { get; }

        /// <summary>
        /// Gets all outgoing expenses from the balance.
        /// </summary>
        public long Outgoing { get; }

        /// <summary>
        /// Gets the current gross amount based on incoming and outcoming amount.
        /// </summary>
        public long Gross { get; }

        /// <summary>
        /// Gets the overspending amount.
        /// </summary>
        public long Overspending { get; }

        /// <summary>
        /// Topup the balance with incoming revenue.
        /// </summary>
        /// <param name="delta">The amount to top up the balance.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the delta is negative.</exception>
        public void Topup(long delta);

        /// <summary>
        /// Spend the balance with outgoing expenses.
        /// </summary>
        /// <param name="delta">The amount to spend from the balance.</param>
        /// <returns>The balance after spending the amount.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the delta is less than 0 or overspending occurs.</exception>
        public long Deduct(long delta);

        /// <summary>
        /// Adjust the balance with incoming and outgoing amounts.
        /// </summary>
        /// <param name="incoming">The delta of incoming amount to adjust.</param>
        /// <param name="outgoing">The delta of outgoing amount to adjust.</param>
        public void Adjust(long? incoming, long? outgoing);

        /// <summary>
        /// Clear the balance with zero amount.
        /// </summary>
        public void Clear();
    }
}
