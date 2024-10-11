// <copyright file="BalanceExceedancePolicyExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The extension methods for the balance exceedance policy.</summary>
    public static class BalanceExceedancePolicyExtensions
    {
        public static long Deduct(this BalanceExceedancePolicy policy, ref long delta, long maximum)
            => policy switch
            {
                BalanceExceedancePolicy.Reject => Reject(ref delta, maximum),
                BalanceExceedancePolicy.Overflow => Overflow(ref delta, maximum),
                BalanceExceedancePolicy.Overdraft => Overdraft(ref delta, maximum),
                _ => throw new ArgumentOutOfRangeException(nameof(policy), "Invalid balance exceedance policy.")
            };

        private static long Reject(ref long delta, long maximum)
        {
            if (delta > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "The amount is over-spending.");
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

        private static long Overdraft(ref long delta, long maximum)
        {
            return 0;
        }
    }
}
