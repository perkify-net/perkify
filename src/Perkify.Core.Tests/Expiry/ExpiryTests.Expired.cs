namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        /*
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsExpired
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace, clock);
            var expected = nowUtcOffset >= 0;
            expiry.IsExpired().Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsExpiredWithSuspensionUtc
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-3, -2, -1, 0, +1)] int nowUtcOffset,
            [CombinatorialValues(-4, -3, -2, -1, 0)] int suspensionUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());
            var suspensionUtc = expiryUtc.AddHours(suspensionUtcOffset);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            var expected = suspensionUtcOffset >= 0;
            expiry.IsExpired().Should().Be(expected);
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
        public void TestOverdue(string nowUtc, string expiryUtcString, string? graceString, string overdueString)
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
        public void TestOverdueWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string overdueString)
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
        */
    }
}
