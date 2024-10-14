namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        /*
        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00","PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        public void TestExpiryRenew(string nowUtc, string expiryUtcString, string? graceString, string duration, bool calendar, string expectedString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expected = InstantPattern.General.Parse(expectedString).Value.ToDateTimeUtc();

            var expiry = new Expiry(expiryUtc, grace, clock);
            var renewal = new Renewal(duration, calendar);
            expiry.Renew(renewal);
            Assert.Equal(expected, expiry.ExpiryUtc);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H", false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H", false)]
        public void TestExpiryRenewNegativeTimeSpan(string nowUtc, string expiryUtcString, string? graceString, string duration, bool calendar)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;

            var expiry = new Expiry(expiryUtc, grace, clock);
            var renewal = new Renewal(duration, calendar);
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H", false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H", false)]
        public void TestExpiryRenewSuspended(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcStribng, string duration, bool calendar)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcStribng).Value.ToDateTimeUtc();

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            var renewal = new Renewal(duration, calendar);
            Assert.False(expiry.IsActive);
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }
        */
    }
}
