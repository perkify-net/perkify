namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var entitlement = new Entitlement(renewal)
            {
                Balance = balance,
                Clock = clock,
            };

            entitlement.AutoRenewalMode.Should().Be(renewal);
            entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            entitlement.Incoming.Should().Be(balance.Incoming);
            entitlement.Outgoing.Should().Be(balance.Outgoing);
            entitlement.Threshold.Should().Be(balance.Threshold);
            entitlement.BalanceType.Should().Be(balance.BalanceType);
            entitlement.Overspending.Should().Be(balance.Overspending);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestBalanceTopup
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
            var entitlement = new Entitlement(renewal)
            {
                Balance = balance,
                Expiry = expiry,
                Clock = clock,
            };

            entitlement.Topup(delta);
            var actualIncomingDelta = entitlement.Incoming - incoming;
            actualIncomingDelta.Should().Be(delta);
            if (expiry != null)
            {
                var actual = entitlement.ExpiryUtc - expiryUtc!.Value;
                var expected = renewal.HasFlag(AutoRenewalMode.Adjust) ? TimeSpan.FromHours(autoRenewalIntervalInHours) : TimeSpan.Zero;
                actual.Should().Be(expected);
            }
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var entitlement = new Entitlement(renewal)
            {
                Balance = balance,
                Expiry = expiry,
                Clock = clock,
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var entitlement = new Entitlement(renewal)
            {
                Balance = balance,
                Expiry = expiry,
                Clock = clock,
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var entitlement = new Entitlement(renewal)
            {
                Balance = balance,
                Expiry = expiry,
                Clock = clock,
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
}
