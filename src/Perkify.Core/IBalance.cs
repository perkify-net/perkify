// <copyright file="IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The interface to maintain a balance amount with threshold for eligibility.</summary>
    /// <typeparam name="T">The type that implements the IBalance interface.</typeparam>
    public interface IBalance<T>
        where T : IBalance<T>
    {
        /// <summary>Gets all incoming revenue to the balance.</summary>
        public long Incoming { get; }

        /// <summary>Gets all outgoing expenses from the balance.</summary>
        public long Outgoing { get; }

        /// <summary>Gets the threshold amount for the balance.</summary>
        public long Threshold { get; }

        /// <summary>Topup the balance with incoming revenue.</summary>
        /// <param name="delta">The amount to top up the balance.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the delta is less than 0.</exception>
        public void Topup(long delta);

        /// <summary>Spend the balance with outgoing expenses.</summary>
        /// <param name="delta">The amount to spend from the balance.</param>
        /// <param name="policy">The overflow processing policy when the balance is exceeded.</param>
        /// <returns>The balance after spending the amount.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the delta is less than 0 or overspending occurs.</exception>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject);

        /// <summary>Adjust the balance with incoming and outgoing amounts.</summary>
        /// <param name="incoming">The incoming amount to adjust the balance.</param>
        /// <param name="outgoing">The outgoing amount to adjust the balance.</param>
        /// <returns>The adjusted balance with incoming and outgoing amounts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the incoming or outgoing amount is less than 0.</exception>
        public T Adjust(long? incoming, long? outgoing);
    }
}
