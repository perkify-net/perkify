namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

using ExpiryStateChangeEventArgs = StateChangeEventArgs<ExpiryState, ExpiryStateOperation>;

public partial class ExpiryTests
{
    [Theory, CombinatorialData]
    public void TestExpiryRenew
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, +2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
        [CombinatorialValues("PT1H", "PT1H!")] string renewal,
        [CombinatorialValues(1)] int expectedExpiryUtcOffsetInHours,
        [CombinatorialValues(true, false)] bool isStateChangedEventHooked
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        expiry.Renewal.Should().BeNull();
        ExpiryStateChangeEventArgs? stateChangedEvent = null;
        if (isStateChangedEventHooked)
        {
            expiry.StateChanged += (sender, e) => { stateChangedEvent = e; };
        }

        expiry.Renew(renewal);
        var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
        expiry.ExpiryUtc.Should().Be(expected);
        expiry.Renewal.Should().NotBeNull();
        expiry.Renewal!.Duration.Should().Be(renewal);
        if (isStateChangedEventHooked)
        {
            stateChangedEvent.Should().NotBeNull();
            stateChangedEvent!.Operation.Should().Be(ExpiryStateOperation.Renew);
            stateChangedEvent!.From.ExpiryUtc.Should().Be(expiryUtc);
            stateChangedEvent!.From.GracePeriod.Should().Be(grace);
            stateChangedEvent!.To.ExpiryUtc.Should().Be(expected);
            stateChangedEvent!.To.GracePeriod.Should().Be(grace);
        }
    }

    [Theory, CombinatorialData]
    public void TestExpiryRenewReusingLastRenewal
    (
         [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
         [CombinatorialValues(0, +2)] int gracePeriodInHours,
         [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
         [CombinatorialValues("PT1H", "PT1H!")] string last,
         [CombinatorialValues(1)] int expectedExpiryUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace }.WithRenewal(last);
        expiry.Renewal.Should().NotBeNull();
        expiry.Renewal!.Duration.Should().Be(last);
        expiry.Renew(null);

        var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
        expiry.ExpiryUtc.Should().Be(expected);
        expiry.Renewal.Should().NotBeNull();
        expiry.Renewal!.Duration.Should().Be(last);
    }

    [Theory, CombinatorialData]
    public void TestExpiryRenewOverridingLastRenewal
    (
         [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
         [CombinatorialValues(0, +2)] int gracePeriodInHours,
         [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
         [CombinatorialValues("P1M")] string last,
         [CombinatorialValues("PT1H", "PT1H!")] string renewal,
         [CombinatorialValues(1)] int expectedExpiryUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace }.WithRenewal(last);
        expiry.Renewal.Should().NotBeNull();
        expiry.Renewal!.Duration.Should().Be(last);
        expiry.Renew(renewal);

        var expected = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
        expiry.ExpiryUtc.Should().Be(expected);
        expiry.Renewal.Should().NotBeNull();
        expiry.Renewal!.Duration.Should().Be(renewal);
    }

    [Theory, CombinatorialData]
    public void TestExpiryRenewMissingLastRenewal
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, +2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
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

    [Theory, CombinatorialData]
    public void TestExpiryRenewNegativeTimeSpan
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, +2)] int gracePeriodInHours,
        [CombinatorialValues(-1)] int nowUtcOffsetInHours,
        [CombinatorialValues("PT-1H", "PT-1H!")] string renewal
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        expiry.ExpiryUtc.Should().Be(expiryUtc);
        expiry.Renewal.Should().BeNull();

        var action = () => expiry.Renew(renewal);
        action.Should()
        .Throw<InvalidOperationException>()
            .WithMessage($"Negative ISO8601 duration.");
        expiry.ExpiryUtc.Should().Be(expiryUtc);
        expiry.Renewal.Should().BeNull();
    }

    [Theory, CombinatorialData]
    public void TestExpiryAdjustTo
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(0, +2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffsetInHours,
        [CombinatorialValues(+10)] int expectedExpiryUtcOffsetInHours,
        [CombinatorialValues(true, false)] bool isStateChangedEventHooked
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        ExpiryStateChangeEventArgs? stateChangedEvent = null;
        if (isStateChangedEventHooked)
        {
            expiry.StateChanged += (sender, e) => { stateChangedEvent = e; };
        }

        var expectedExpiryUtc = expiryUtc.AddHours(expectedExpiryUtcOffsetInHours);
        expiry.AdjustTo(expectedExpiryUtc);
        expiry.ExpiryUtc.Should().Be(expectedExpiryUtc);
        if (isStateChangedEventHooked)
        {
            stateChangedEvent.Should().NotBeNull();
            stateChangedEvent!.Operation.Should().Be(ExpiryStateOperation.Adjust);
            stateChangedEvent!.From.ExpiryUtc.Should().Be(expiryUtc);
            stateChangedEvent!.From.GracePeriod.Should().Be(grace);
            stateChangedEvent!.To.ExpiryUtc.Should().Be(expectedExpiryUtc);
            stateChangedEvent!.To.GracePeriod.Should().Be(grace);
        }
    }
}
