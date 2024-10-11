// <copyright file="Renewal.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>The renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
    public class Renewal
    {
        private Period period;

        /// <summary>Gets the ISO8601 duration string.</summary>
        public string Duration { get; private set; }

        /// <summary>Gets a value indicating whether to run date/time calculationc in calendar arithmetic.</summary>
        public bool Calendar { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Renewal"/> class.Create a renewal period based on ISO8601 duration string and flag to identify calendar arithmetic.</summary>
        /// <param name="duration">The ISO8601 duration string.</param>
        /// <param name="calendar">The flag to identify calendar arithmetic.</param>
        public Renewal(string duration, bool calendar = false)
        {
            var result = PeriodPattern.NormalizingIso.Parse(duration);
            if (!result.Success)
            {
                throw new FormatException("Incorrect ISO8601 duration string.", result.Exception);
            }

            this.period = result.Value.Normalize();
            this.Duration = duration;
            this.Calendar = calendar;
        }

        /// <summary>Renew the expiry time in timeline arithmetic or calendrical arithmetic.</summary>
        /// <param name="expiryUtc">The original expiry time in UTC.</param>
        /// <returns>The expiry time after renewal.</returns>
        public DateTime Renew(DateTime expiryUtc)
        {
            if (this.Calendar)
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

        /// <summary>Calculate the timespan between 2 expiry times with the renewal period.</summary>
        /// <param name="expiryUtc">The final expiry time in UTC updated after renewal.</param>
        /// <returns>The elapsed timespan between 2 expiry times.</returns>
        public TimeSpan Till(DateTime expiryUtc)
        {
            if (this.Calendar)
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
