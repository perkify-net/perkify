namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

using ExpiryStateChangeEventArgs = StateChangeEventArgs<ExpiryState, ExpiryStateOperation>;

public partial class EntitlementTests
{
    [Theory, CombinatorialData]
    public void TestExpiryProperties
    (
        [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
        [CombinatorialValues(0, 2)] int gracePeriodInHours
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        var entitlement = new Entitlement(renewal, clock)
        {
            Expiry = expiry,
        };
        entitlement.AutoRenewalMode.Should().Be(renewal);
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
        entitlement.ExpiryUtc.Should().Be(expiry.ExpiryUtc);
        entitlement.GracePeriod.Should().Be(expiry.GracePeriod);
        entitlement.DeadlineUtc.Should().Be(expiry.DeadlineUtc);
        entitlement.IsExpired.Should().Be(expiry.IsExpired);
        entitlement.Overdue.Should().Be(expiry.Overdue);
        entitlement.Renewal.Should().Be(expiry.Renewal);
        entitlement.Remaining(false).Should().Be(expiry.Remaining(false));
        entitlement.Remaining(true).Should().Be(expiry.Remaining(true));
    }

    [Theory, CombinatorialData]
    public void TestExpirySetGracePeriod
    (
        [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
        [CombinatorialValues(2)] int gracePeriodInHours
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var expiry = new Expiry(expiryUtc) { GracePeriod = grace };

        var entitlement = new Entitlement(renewal, clock)
        {
            Expiry = expiry,
        };
        entitlement.GracePeriod.Should().Be(expiry.GracePeriod);
        entitlement.GracePeriod = TimeSpan.Zero;
        entitlement.GracePeriod.Should().Be(TimeSpan.Zero);
    }

    [Theory, CombinatorialData]
    public void TestExpiryRenew
    (
        [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
        [CombinatorialValues(2)] int gracePeriodInHours,
        [CombinatorialValues(1)] int renewalIntervalInHours,
        [CombinatorialValues(true, false)] bool isStateChangedEventHooked
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };

        var entitlement = new Entitlement(renewal, clock)
        {
            Expiry = expiry,
        };
        ExpiryStateChangeEventArgs? stateChangedEvent = null;
        if (isStateChangedEventHooked)
        {
            entitlement.ExpiryStateChanged += (sender, e) => { stateChangedEvent = e; };
        }

        entitlement.Renew($"PT{renewalIntervalInHours}H!");
        var actual = entitlement.ExpiryUtc - expiryUtc;
        actual.Hours.Should().Be(renewalIntervalInHours);
        if (isStateChangedEventHooked)
        {
            stateChangedEvent.Should().NotBeNull();
            stateChangedEvent!.Operation.Should().Be(ExpiryStateOperation.Renew);
            stateChangedEvent!.From.ExpiryUtc.Should().Be(expiryUtc);
            stateChangedEvent!.From.GracePeriod.Should().Be(grace);
            stateChangedEvent!.To.ExpiryUtc.Should().Be(entitlement.ExpiryUtc);
            stateChangedEvent!.To.GracePeriod.Should().Be(grace);
        }
    }

    [Theory, CombinatorialData]
    public void TestExpiryAdjustTo
    (
        [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
        [CombinatorialValues(2)] int gracePeriodInHours,
        [CombinatorialValues(+10)] int expectedExpiryUtcOffsetInHours
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };

        var entitlement = new Entitlement(renewal, clock)
        {
            Expiry = expiry,
        };
        var expectedExpiryUtc = nowUtc.AddHours(expectedExpiryUtcOffsetInHours);
        entitlement.AdjustTo(expectedExpiryUtc);
        entitlement.ExpiryUtc.Should().Be(expectedExpiryUtc);
    }
}
