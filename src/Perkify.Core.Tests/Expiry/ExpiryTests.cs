namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateExpiry
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

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock };
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.GracePeriod.Should().Be(grace ?? TimeSpan.Zero);
            expiry.NowUtc.Should().Be(nowUtc);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateExpiryWithSystemClock
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;

            var expiry = new Expiry(expiryUtc, grace);
            expiry.ExpiryUtc.Should().Be(expiryUtc);
            expiry.GracePeriod.Should().Be(grace ?? TimeSpan.Zero);
            expiry.NowUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateExpiryWithRenewal
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-1)] int nowUtcOffset,
            [CombinatorialValues("P1M!", "PT1H!", "PT1H")] string duration
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());
            var calendar = !duration.EndsWith('!');

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock }.WithRenewal(duration);
            expiry.Renewal!.Calendar.Should().Be(calendar);
            expiry.Renewal!.Duration.Should().Be(duration);
        }
    }
}
