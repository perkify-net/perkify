// <copyright file="ChronoInterval.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// The renewal interval based on ISO8601 duration string and flag to identify calendar arithmetic.
    /// </summary>
    public class ChronoInterval
    {
        private readonly string duration;

        private readonly Period period;

        private readonly bool calendar;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChronoInterval"/> class with a specified ISO8601 duration string.
        /// </summary>
        /// <param name="iso8601">The ISO8601 duration string.</param>
        public ChronoInterval(string iso8601)
        {
            string duration = iso8601?.Trim() ?? throw new ArgumentNullException(nameof(iso8601));
            var calendar = !duration.EndsWith('!');

            var result = PeriodPattern.NormalizingIso.Parse(duration.TrimEnd('!'));
            if (!result.Success)
            {
                throw new FormatException("Incorrect ISO8601 duration string.", result.Exception);
            }

            this.duration = duration;
            this.period = result.Value.Normalize();
            this.calendar = calendar;
        }

        /// <summary>
        /// Gets the ISO8601 duration string.
        /// </summary>
        public string Duration => this.duration;

        /// <summary>
        /// Gets a value indicating whether calendar arithmetic is used.
        /// </summary>
        public bool Calendar => this.calendar;

        /// <summary>
        /// Renew the expiry time in timeline arithmetic or calendrical arithmetic.
        /// </summary>
        /// <param name="expiryUtc">The original expiry time in UTC.</param>
        /// <returns>The expiry time after renewal.</returns>
        public DateTime Renew(DateTime expiryUtc)
        {
            if (this.calendar)
            {
                var current = Instant.FromDateTimeUtc(expiryUtc).InZone(DateTimeZone.Utc).LocalDateTime;
                var future = current + this.period;
                return future.InZoneStrictly(DateTimeZone.Utc).ToInstant().ToDateTimeUtc();
            }
            else
            {
                var ts = this.period.ToDuration().ToTimeSpan();
                return expiryUtc + ts;
            }
        }

        /// <summary>
        /// Calculate the timespan between 2 expiry times with the renewal period.
        /// </summary>
        /// <param name="expiryUtc">The final expiry time in UTC updated after renewal.</param>
        /// <returns>The elapsed timespan between 2 expiry times.</returns>
        public TimeSpan Till(DateTime expiryUtc)
        {
            if (this.calendar)
            {
                var nextExpiryUtc = expiryUtc;
                var nextExpiry = Instant.FromDateTimeUtc(expiryUtc).InZone(DateTimeZone.Utc).LocalDateTime;
                var previousExpiry = nextExpiry - this.period;
                var previousExpiryUtc = previousExpiry.InZoneStrictly(DateTimeZone.Utc).ToInstant().ToDateTimeUtc();
                return nextExpiryUtc - previousExpiryUtc;
            }
            else
            {
                var ts = this.period.ToDuration().ToTimeSpan();
                return ts;
            }
        }
    }
}
