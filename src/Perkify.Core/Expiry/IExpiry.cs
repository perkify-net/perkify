// <copyright file="IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// The interface to maintain an expiry time with grace period for eligibility.
    /// </summary>
    public interface IExpiry
    {
        /// <summary>
        /// Gets the expiry time in UTC.
        /// </summary>
        public DateTime ExpiryUtc { get; }

        /// <summary>
        /// Gets the Grace period as absolute time span.
        /// </summary>
        public TimeSpan GracePeriod { get; }

        /// <summary>
        /// Gets calculates the deadline time in UTC based on the expiry time and grace period.
        /// </summary>
        public DateTime DeadlineUtc { get; }

        /// <summary>
        /// Gets a value indicating whether the expiry time has been reached.
        /// </summary>
        public bool IsExpired { get; }

        /// <summary>
        /// Gets the remaining portion.
        /// - If suspended, the remaining portion is the time between suspend time and expiry time.
        /// - If eligible, the remaining portion is the current time between now and expiry time.
        /// - If ineligible (after grace period), the remaining portion is negative grace period.
        /// </summary>
        public TimeSpan Remaining { get; }

        /// <summary>
        /// Gets the overdue portion.
        /// - Suspended: The overdue portion is the time between suspend time and expiry time. Zero if suspend time is earlier than expiry time.
        /// - Eligible: The overdue portion is the time between expiry time and now. Zero if now is earlier than expiry time.
        /// - Ineligible (after grace period): The overdue portion is the grace period.
        /// </summary>
        public TimeSpan Overdue { get; }

        /// <summary>
        /// Gets the renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.
        /// </summary>
        public ChronoInterval? Renewal { get; }

        /// <summary>
        /// Renew the expiry time in timeline arithmetic or calendrical arithmetic.
        /// </summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        public void Renew(ChronoInterval? renewal);
    }
}
