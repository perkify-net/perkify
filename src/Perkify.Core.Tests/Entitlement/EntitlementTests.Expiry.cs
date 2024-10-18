namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryProperties
        (
            [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
            [CombinatorialValues(null, 2)] int? gracePeriodInHoursIfHaving
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
            var grace = gracePeriodInHoursIfHaving != null ? TimeSpan.FromHours(gracePeriodInHoursIfHaving.Value) : (TimeSpan?)null;
            var expiry = new Expiry(expiryUtc, grace);

            var entitlement = new Entitlement(renewal)
            {
                Expiry = expiry,
                Clock = clock,
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var expiry = new Expiry(expiryUtc, grace);

            var entitlement = new Entitlement(renewal)
            {
                Expiry = expiry,
                Clock = clock,
            };
            entitlement.GracePeriod.Should().Be(expiry.GracePeriod);
            entitlement.GracePeriod = null;
            entitlement.GracePeriod.Should().BeNull();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestExpiryRenew
        (
            [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(+5)] int expiryUtcOffsetInHours,
            [CombinatorialValues(2)] int gracePeriodInHours,
            [CombinatorialValues(1)] int renewalIntervalInHours
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
            var grace = TimeSpan.FromHours(gracePeriodInHours);
            var expiry = new Expiry(expiryUtc, grace);

            var entitlement = new Entitlement(renewal)
            {
                Expiry = expiry,
                Clock = clock,
            };
            entitlement.Renew($"PT{renewalIntervalInHours}H!");
            var actual = entitlement.ExpiryUtc - expiryUtc;
            actual.Hours.Should().Be(renewalIntervalInHours);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            var expiry = new Expiry(expiryUtc, grace);

            var entitlement = new Entitlement(renewal)
            {
                Expiry = expiry,
                Clock = clock,
            };
            var expectedExpiryUtc = nowUtc.AddHours(expectedExpiryUtcOffsetInHours);
            entitlement.AdjustTo(expectedExpiryUtc);
            entitlement.ExpiryUtc.Should().Be(expectedExpiryUtc);
        }
    }
}
