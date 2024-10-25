namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

public partial class ExpiryTests
{
    [Theory, CombinatorialData]
    public void TestCreateExpiry
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, 2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffset
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffset);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc) { Clock = clock, GracePeriod = grace };
        expiry.ExpiryUtc.Should().Be(expiryUtc);
        expiry.GracePeriod.Should().Be(grace);
        expiry.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
    }

    [Theory, CombinatorialData]
    public void TestCreateExpiryWithSystemClock
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, 2)] int gracePeriodInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);

        var expiry = new Expiry(expiryUtc) { GracePeriod = grace } ;
        expiry.ExpiryUtc.Should().Be(expiryUtc);
        expiry.GracePeriod.Should().Be(grace);
        expiry.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
    }

    [Theory, CombinatorialData]
    public void TestCreateExpiryWithRenewal
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, 2)] int gracePeriodInHours,
        [CombinatorialValues(-1)] int nowUtcOffset,
        [CombinatorialValues("P1M!", "PT1H!", "PT1H")] string duration
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffset);
        var clock = new FakeClock(nowUtc.ToInstant());
        var calendar = !duration.EndsWith('!');

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace }.WithRenewal(duration);
        expiry.Renewal!.Calendar.Should().Be(calendar);
        expiry.Renewal!.Duration.Should().Be(duration);
    }
}
