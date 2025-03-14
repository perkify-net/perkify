namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

using BalanceStateChangeEventArgs = StateChangeEventArgs<BalanceState, BalanceStateOperation>;

public partial class EntitlementTests
{
    [Theory, CombinatorialData]
    public void TestBalanceProperties
    (
        [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(0L)] long threshold,
        [CombinatorialValues(100L)] long incoming,
        [CombinatorialValues(50L)] long outgoing
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = balance,
        };

        entitlement.AutoRenewalMode.Should().Be(renewal);
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
        entitlement.Threshold.Should().Be(balance.Threshold);
        entitlement.BalanceExceedancePolicy.Should().Be(balance.BalanceExceedancePolicy);
        entitlement.BalanceType.Should().Be(balance.BalanceType);
        entitlement.Incoming.Should().Be(balance.Incoming);
        entitlement.Outgoing.Should().Be(balance.Outgoing);
        entitlement.Gross.Should().Be(balance.Gross);
        entitlement.Available.Should().Be(balance.Available);
        entitlement.Overspending.Should().Be(balance.Overspending);
    }

    [Theory, CombinatorialData]
    public void TestBalanceTopup
    (
        [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.All)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(0L)] long threshold,
        [CombinatorialValues(100L)] long incoming,
        [CombinatorialValues(50L)] long outgoing,
        [CombinatorialValues(null, +5)] int? expiryUtcOffsetInHoursIfHaving,
        [CombinatorialValues(1)] int autoRenewalIntervalInHours,
        [CombinatorialValues(10L)] long delta,
        [CombinatorialValues(true, false)] bool isStateChangedEventHooked
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        var expiryUtc = expiryUtcOffsetInHoursIfHaving != null ? nowUtc.AddHours(expiryUtcOffsetInHoursIfHaving.Value) : (DateTime?)null;
        var expiry = expiryUtc != null ? new Expiry(expiryUtc.Value, null).WithRenewal($"PT{autoRenewalIntervalInHours}H!") : null;
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = balance,
            Expiry = expiry,
        };
        BalanceStateChangeEventArgs? stateChangedEvent = null;
        if (isStateChangedEventHooked)
        {
            entitlement.BalanceStateChanged += (sender, e) => { stateChangedEvent = e; };
        }

        entitlement.Topup(delta);
        var actualIncomingDelta = entitlement.Incoming - incoming;
        actualIncomingDelta.Should().Be(delta);
        if (expiry != null)
        {
            var actual = entitlement.ExpiryUtc - expiryUtc!.Value;
            var expected = renewal.HasFlag(AutoRenewalMode.Adjust) ? TimeSpan.FromHours(autoRenewalIntervalInHours) : TimeSpan.Zero;
            actual.Should().Be(expected);
        }
        if (isStateChangedEventHooked)
        {
            stateChangedEvent.Should().NotBeNull();
            stateChangedEvent!.Operation.Should().Be(BalanceStateOperation.Topup);
            stateChangedEvent!.From.BalanceExceedancePolicy.Should().Be(BalanceExceedancePolicy.Reject);
            stateChangedEvent!.From.Threshold.Should().Be(threshold);
            stateChangedEvent!.From.Incoming.Should().Be(incoming);
            stateChangedEvent!.From.Outgoing.Should().Be(outgoing);
            stateChangedEvent!.To.BalanceExceedancePolicy.Should().Be(BalanceExceedancePolicy.Reject);
            stateChangedEvent!.To.Threshold.Should().Be(threshold);
            stateChangedEvent!.To.Incoming.Should().Be(entitlement.Incoming);
            stateChangedEvent!.To.Outgoing.Should().Be(outgoing);
        }
    }

    [Theory, CombinatorialData]
    public void TestBalanceDeduct
    (
        [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.All)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(0L)] long threshold,
        [CombinatorialValues(100L)] long incoming,
        [CombinatorialValues(50L)] long outgoing,
        [CombinatorialValues(null, +5)] int? expiryUtcOffsetInHoursIfHaving,
        [CombinatorialValues(1)] int autoRenewalIntervalInHours,
        [CombinatorialValues(10L)] long delta
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        var expiryUtc = expiryUtcOffsetInHoursIfHaving != null ? nowUtc.AddHours(expiryUtcOffsetInHoursIfHaving.Value) : (DateTime?)null;
        var expiry = expiryUtc != null ? new Expiry(expiryUtc.Value, null).WithRenewal($"PT{autoRenewalIntervalInHours}H!") : null;
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = balance,
            Expiry = expiry,
        };

        entitlement.Deduct(delta);
        var actualOutgoingDelta = entitlement.Outgoing - outgoing;
        actualOutgoingDelta.Should().Be(delta);
        if (expiry != null)
        {
            var actual = entitlement.ExpiryUtc - expiryUtc!.Value;
            var expected = renewal.HasFlag(AutoRenewalMode.Adjust) ? TimeSpan.FromHours(autoRenewalIntervalInHours) : TimeSpan.Zero;
            actual.Should().Be(expected);
        }
    }

    [Theory, CombinatorialData]
    public void TestBalanceAdjust
    (
        [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.All)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(0L)] long threshold,
        [CombinatorialValues(100L)] long incoming,
        [CombinatorialValues(50L)] long outgoing,
        [CombinatorialValues(null, +5)] int? expiryUtcOffsetInHoursIfHaving,
        [CombinatorialValues(1)] int autoRenewalIntervalInHours,
        [CombinatorialValues(null, 10L)] long? adjustedIncomingDelta,
        [CombinatorialValues(null, 10L)] long? adjustedOutgoingDelta
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        var expiryUtc = expiryUtcOffsetInHoursIfHaving != null ? nowUtc.AddHours(expiryUtcOffsetInHoursIfHaving.Value) : (DateTime?)null;
        var expiry = expiryUtc != null ? new Expiry(expiryUtc.Value, null).WithRenewal($"PT{autoRenewalIntervalInHours}H!") : null;
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = balance,
            Expiry = expiry,
        };

        entitlement.Adjust(adjustedIncomingDelta, adjustedOutgoingDelta);
        var actualAdjustedIncomingDelta = entitlement.Incoming - incoming;
        var actualAdjustedOutgoingDelta = entitlement.Outgoing - outgoing;
        actualAdjustedIncomingDelta.Should().Be(adjustedIncomingDelta ?? 0L);
        actualAdjustedOutgoingDelta.Should().Be(adjustedOutgoingDelta ?? 0L);
        if (expiry != null)
        {
            var actual = entitlement.ExpiryUtc - expiryUtc!.Value;
            var expected = renewal.HasFlag(AutoRenewalMode.Adjust) ? TimeSpan.FromHours(autoRenewalIntervalInHours) : TimeSpan.Zero;
            actual.Should().Be(expected);
        }
    }

    [Theory, CombinatorialData]
    public void TestBalanceClear
    (
        [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.All)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
        [CombinatorialValues(0L)] long threshold,
        [CombinatorialValues(100L)] long incoming,
        [CombinatorialValues(50L)] long outgoing,
        [CombinatorialValues(null, +5)] int? expiryUtcOffsetInHoursIfHaving,
        [CombinatorialValues(1)] int autoRenewalIntervalInHours
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        var expiryUtc = expiryUtcOffsetInHoursIfHaving != null ? nowUtc.AddHours(expiryUtcOffsetInHoursIfHaving.Value) : (DateTime?)null;
        var expiry = expiryUtc != null ? new Expiry(expiryUtc.Value, null).WithRenewal($"PT{autoRenewalIntervalInHours}H!") : null;
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = balance,
            Expiry = expiry,
        };

        entitlement.Clear();
        entitlement.Incoming.Should().Be(0L);
        entitlement.Outgoing.Should().Be(0L);
        if(expiry != null)
        {
            var actual = entitlement.ExpiryUtc - expiryUtc!.Value;
            var expected = renewal.HasFlag(AutoRenewalMode.Adjust) ? TimeSpan.FromHours(autoRenewalIntervalInHours) : TimeSpan.Zero;
            actual.Should().Be(expected);
        }
    }
}
