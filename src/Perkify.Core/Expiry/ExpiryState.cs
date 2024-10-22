// <copyright file="ExpiryState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents the state of an expiry with a specific UTC date and time and an optional grace period.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExpiryState"/> class with the specified expiry date and time.
    /// </remarks>
    /// <param name="expiryUtc">The UTC date and time when the expiry occurs.</param>
    public class ExpiryState(DateTime expiryUtc)
    {
        /// <summary>
        /// Gets the UTC date and time when the expiry occurs.
        /// </summary>
        public DateTime ExpiryUtc { get; } = expiryUtc;

        /// <summary>
        /// Gets the grace period after the expiry date.
        /// </summary>
        public TimeSpan GracePeriod { get; init; }
    }
}
