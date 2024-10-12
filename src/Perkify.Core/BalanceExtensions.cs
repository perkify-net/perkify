﻿// <copyright file="BalanceExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Provides extension methods for balance operations.
    /// </summary>
    public static class BalanceExtensions
    {
        /// <summary>
        /// The balance type: debit or credit.
        /// </summary>
        /// <typeparam name="T">The type of the balance.</typeparam>
        /// <param name="balance">The balance instance.</param>
        /// <returns>The balance type.</returns>
        public static BalanceType GetBalanceType<T>(this T balance)
            where T : IBalance<T>
            => balance.Threshold >= 0 ? BalanceType.Debit : BalanceType.Credit;

        /// <summary>
        /// The current balance amount based on incoming &amp; outcoming amount.
        /// </summary>
        /// <typeparam name="T">The type of the balance.</typeparam>
        /// <param name="balance">The balance instance.</param>
        /// <returns>The current balance amount.</returns>
        public static long GetBalanceAmount<T>(this T balance)
            where T : IBalance<T>
            => balance.Incoming - balance.Outgoing;

        /// <summary>
        /// The over spending amount.
        /// </summary>
        /// <typeparam name="T">The type of the balance.</typeparam>
        /// <param name="balance">The balance instance.</param>
        /// <returns>The amount overspent beyond the threshold.</returns>
        public static long GetOverspendingAmount<T>(this T balance)
            where T : IBalance<T>
            => balance.GetBalanceAmount() >= balance.Threshold ? 0 : balance.Threshold - balance.GetBalanceAmount();

        /// <summary>
        /// The upper limit amount that can be deducted from the account balance.
        /// </summary>
        /// <typeparam name="T">The type of the balance.</typeparam>
        /// <param name="balance">The balance instance.</param>
        /// <param name="policy">The policy for balance exceedance.</param>
        /// <returns>The maximum deductible amount.</returns>
        public static long GetMaxDeductibleAmount<T>(this T balance, BalanceExceedancePolicy policy)
            where T : IBalance<T>
            => policy != BalanceExceedancePolicy.Overdraft
            ? Math.Max(balance.GetBalanceAmount() - balance.Threshold, 0L)
            : long.MaxValue;
    }
}
