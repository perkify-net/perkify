namespace Perkify.Core
{
    /// <summary>The interface to maintain an expiry time with grace period for eligibility.</summary>
    public interface IExpiry<T> where T: IExpiry<T>, INowUtc
    {
        #region IsExpired, Remaining & Overdue

        /// <summary>The Grace period as absolute time span.</summary>
        public TimeSpan GracePeriod { get; }

        /// <summary>
        /// The remaining portion.
        /// - If suspended, the remaining portion is the time between suspend time and expiry time.
        /// - If eligible, the remaining portion is the current time between now and expiry time.
        /// - If ineligible (after grace period), the remaining portion is negative grace period.
        /// </summary>
        public TimeSpan Remaining { get; }

        /// <summary>
        /// The overdue portion.
        /// - Suspended: The overdue portion is the time between suspend time and expiry time. Zero if suspend time is earlier than expiry time.
        /// - Eligible: The overdue portion is the time between expiry time and now. Zero if now is earlier than expiry time.
        /// - Ineligible (after grace period): The overdue portion is the grace period.
        /// </summary>
        public TimeSpan Overdue { get; }

        #endregion

        #region Renew

        /// <summary>The expiry time in UTC.</summary>
        public DateTime ExpiryUtc { get; }

        /// <summary>The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
        public Renewal Renewal { get; }

        /// <summary>Renew the expiry time in timeline arithmetic or calendrical arithmetic.</summary>
        /// <param name="renewal">The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</param>
        /// <returns>The expiry time after renewal.</returns>
        public T Renew(Renewal? renewal);

        #endregion

        #region Deactivate & Activate

        /// <summary>
        /// The suspend time in UTC.
        /// - Explicit suspension time (smaller or equals to deadline time) if any suspension is applied.
        /// - Implicit suspension time (equals to deadline time) when the expiry time is ineligible.
        /// - Null when the expiry time is eligible (and no suspension is applied).
        /// </summary>
        public DateTime? SuspensionUtc { get; }

        /// <summary>
        /// Boolean flag to identify if the expiry time is active.
        /// - True if the expiry time is deactivated (suspended).
        /// - False if the expiry time is activated (not suspended).
        /// </summary>
        public bool IsActive { get; }

        /// <summary>Deactivate the expiry time.</summary>
        /// <param name="suspendUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The suspension time must be earlier than current time.</exception>
        public T Deactivate(DateTime? suspensionUtc = null);

        /// <summary>Activate the expiry time.</summary>
        /// <param name="resumptionUtc">The resumption time in UTC.</param>
        /// <param name="extended">Boolean flag to extend the expiry time after resumption.</param>
        /// <returns>The expiry time after resumption.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Resume time must be greater than suspend time.</exception>
        public T Activate(DateTime? resumptionUtc = null, bool extended = false);

        #endregion
    }

    public static class ExpiryExtensions
    {
        /// <summary>
        /// The deadline time in UTC.
        /// </summary>
        public static DateTime GetDeadlineUtc<T>(this T expiry) where T: IExpiry<T>, INowUtc
            => expiry.ExpiryUtc + expiry.GracePeriod;

        /// <summary>The boolean flag to identify if the expiry time has been reached.</summary>
        public static bool IsExpired<T>(this T expiry) where T: IExpiry<T>, INowUtc
            => (expiry.SuspensionUtc ?? expiry.NowUtc) >= expiry.ExpiryUtc;
    }
}