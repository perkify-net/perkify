// <copyright file="ExpiryExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Provides extension methods for expiry-related operations.
    /// </summary>
    public static class ExpiryExtensions
    {
        /// <summary>
        /// Calculates the deadline time in UTC based on the expiry object.
        /// </summary>
        /// <typeparam name="T">The type of the expiry object.</typeparam>
        /// <param name="expiry">The expiry object.</param>
        /// <returns>The deadline time in UTC.</returns>
        public static DateTime GetDeadlineUtc<T>(this T expiry)
            where T : IExpiry<T>, INowUtc
            => expiry.ExpiryUtc + expiry.GracePeriod;

        /// <summary>
        /// Determines if the expiry time has been reached.
        /// </summary>
        /// <typeparam name="T">The type of the expiry object.</typeparam>
        /// <param name="expiry">The expiry object.</param>
        /// <returns>True if the expiry time has been reached; otherwise, false.</returns>
        public static bool IsExpired<T>(this T expiry)
            where T : IExpiry<T>, INowUtc
            => (expiry.SuspensionUtc ?? expiry.NowUtc) >= expiry.ExpiryUtc;
    }
}