// <copyright file="BalanceExceedancePolicyExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The extension methods for the balance exceedance policy.
    /// </summary>
    public static class BalanceExceedancePolicyExtensions
    {
        /// <summary>
        /// Deducts the specified delta from the balance based on the given policy and maximum limit.
        /// </summary>
        /// <param name="policy">The balance exceedance policy to apply.</param>
        /// <param name="delta">The amount to deduct, passed by reference.</param>
        /// <param name="maximum">The maximum allowable balance.</param>
        /// <returns>The remaining balance after deduction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the policy is invalid or the delta exceeds the maximum in Reject policy.</exception>
        public static long Deduct(this BalanceExceedancePolicy policy, ref long delta, long maximum)
            => policy switch
            {
                BalanceExceedancePolicy.Reject => Reject(ref delta, maximum),
                BalanceExceedancePolicy.Overflow => Overflow(ref delta, maximum),
                BalanceExceedancePolicy.Overdraft => Overdraft(ref delta, maximum),
                _ => throw new NotSupportedException($"Unsupported balance exceedance policy: {policy}")
            };

        private static long Reject(ref long delta, long maximum)
        {
            if (delta > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Rejected due to insufficient balance.");
            }

            return 0;
        }

        private static long Overflow(ref long delta, long maximum)
        {
            var processed = delta <= maximum ? delta : maximum;
            var remainning = delta - processed;
            delta = processed;
            return remainning;
        }

        private static long Overdraft(ref long delta, long maximum) => 0;
    }
}
