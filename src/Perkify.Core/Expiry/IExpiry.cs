// <copyright file="IExpiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

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
    /// Gets or sets the Grace period as absolute time span.
    /// </summary>
    public TimeSpan GracePeriod { get; set; }

    /// <summary>
    /// Gets calculates the deadline time in UTC based on the expiry time and grace period.
    /// </summary>
    public DateTime DeadlineUtc { get; }

    /// <summary>
    /// Gets a value indicating whether the expiry time has been reached.
    /// </summary>
    public bool IsExpired { get; }

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
    /// Gets the remaining time until the expiry or deadline.
    /// </summary>
    /// <param name="deadline">If true, calculates the remaining time until the deadline; otherwise, calculates the remaining time until the expiry.</param>
    /// <returns>The remaining time as a <see cref="TimeSpan"/>.</returns>
    public TimeSpan Remaining(bool deadline = false);

    /// <summary>
    /// Renews the expiry time using either timeline or calendrical arithmetic.
    /// </summary>
    /// <param name="interval">The renewal interval, specified as an ISO 8601 duration string.</param>
    public void Renew(string? interval);

    /// <summary>
    /// Adjusts the expiry time to the specified UTC time.
    /// </summary>
    /// <param name="expiryUtc">The new expiry time in UTC.</param>
    public void AdjustTo(DateTime expiryUtc);
}
