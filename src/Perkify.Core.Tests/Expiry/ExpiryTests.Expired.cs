namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        public void TestExpiryExpired(string nowUtcString, string expiryUtcString, string? graceString, bool expired)
        {
            var clock = nowUtcString != null ? new FakeClock(InstantPattern.General.Parse(nowUtcString).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);
            Assert.Equal(expired, expiry.IsExpired());
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", true)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", true)]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", true)]
        public void TestExpiryExpiredWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, bool expired)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expired, expiry.IsExpired());
        }


        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "01:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "02:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "02:00:00")]
        public void TestExpiryOverdue(string nowUtc, string expiryUtcString, string? graceString, string overdueString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var overdue = TimeSpan.Parse(overdueString, CultureInfo.InvariantCulture);

            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(overdue, expiry.Overdue);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", "02:00:00")]
        public void TestExpiryOverdueWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string overdueString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var overdue = TimeSpan.Parse(overdueString, CultureInfo.InvariantCulture);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(overdue, expiry.Overdue);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "01:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "01:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-01:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-02:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-02:00:00")]
        public void TestExpiryRemaining(string nowUtc, string expiryUtcString, string? graceString, string remainingString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var remaining = TimeSpan.Parse(remainingString, CultureInfo.InvariantCulture);
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(remaining, expiry.Remaining);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T14:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T14:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", "-01:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", "-02:00:00")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", "-02:00:00")]
        public void TestExpiryRemainingWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string remainingString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var remaining = TimeSpan.Parse(remainingString, CultureInfo.InvariantCulture);
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(remaining, expiry.Remaining);
        }
    }
}
