namespace Perkify.Core.Tests
{
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
        public void TestExpiryRenewReusingLastRenewal
        (
             [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
             [CombinatorialValues(null, +2)] int? gracePeriodInHours,
             [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
             [CombinatorialValues("PT1H", "PT1H!")] string last,
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
            expiry.Renew(null);

            var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
            expiry.ExpiryUtc.Should().Be(expected);
            expiry.Renewal.Should().NotBeNull();
            expiry.Renewal!.Duration.Should().Be(last);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenewOverridingLastRenewal
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenewMissingLastRenewal
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, +2)] int? gracePeriodInHours,
            [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodInHours != null ? TimeSpan.FromHours(gracePeriodInHours.Value) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! };
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.Renewal.Should().BeNull();

            var action = () => expiry.Renew(null);
            var parameter = "interval";
            action.Should()
            .Throw<ArgumentNullException>()
                .WithMessage($"Renewal interval is required. (Parameter '{parameter}')");
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.Renewal.Should().BeNull();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenewNegativeTimeSpan
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, +2)] int? gracePeriodInHours,
            [CombinatorialValues(-1)] int nowUtcOffsetInHours,
            [CombinatorialValues("PT-1H", "PT-1H!")] string renewal
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodInHours != null ? TimeSpan.FromHours(gracePeriodInHours.Value) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! };
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.Renewal.Should().BeNull();

            var action = () => expiry.Renew(renewal);
            action.Should()
            .Throw<InvalidOperationException>()
                .WithMessage($"Negative ISO8601 duration.");
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.Renewal.Should().BeNull();
        }
    }
}
