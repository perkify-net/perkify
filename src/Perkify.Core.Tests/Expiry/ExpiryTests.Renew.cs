namespace Perkify.Core.Tests
{
    using System.Globalization;
    using Bogus;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenew
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, +2)] int? gracePeriodInHours,
            [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
            [CombinatorialValues("PT1H", "PT1H!")] string renewal,
            [CombinatorialValues(1)] int expectedExpiryUtcOffsetInHours
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodInHours != null ? TimeSpan.FromHours(gracePeriodInHours.Value) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! };
            expiry.Renewal.Should().BeNull();
            expiry.Renew(renewal);

            var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
            expiry.ExpiryUtc.Should().Be(expected);
            expiry.Renewal.Should().NotBeNull();
            expiry.Renewal!.Duration.Should().Be(renewal);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenewOverridingRenewal
        (
             [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
             [CombinatorialValues(null, +2)] int? gracePeriodInHours,
             [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
             [CombinatorialValues("P1M")] string last,
             [CombinatorialValues("PT1H", "PT1H!")] string renewal,
             [CombinatorialValues(1)] int expectedExpiryUtcOffsetInHours
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodInHours != null ? TimeSpan.FromHours(gracePeriodInHours.Value) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! }.WithRenewal(last);
            expiry.Renewal.Should().NotBeNull();
            expiry.Renewal!.Duration.Should().Be(last);
            expiry.Renew(renewal);

            var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
            expiry.ExpiryUtc.Should().Be(expected);
            expiry.Renewal.Should().NotBeNull();
            expiry.Renewal!.Duration.Should().Be(renewal);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H!")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H!")]
        public void TestExpiryRenewNegativeTimeSpan(string nowUtc, string expiryUtcString, string? graceString, string renewal)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! };
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }
    }
}
