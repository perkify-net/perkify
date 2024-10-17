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
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expiry = new Expiry(expiryUtc, grace);

            var entitlement = new Entitlement(renewal)
            {
                Expiry = expiry,
                Clock = clock,
            };
            entitlement.AutoRenewalMode.Should().Be(renewal);
            entitlement.NowUtc.Should().Be(nowUtc);
            entitlement.ExpiryUtc.Should().Be(expiry.ExpiryUtc);
            entitlement.GracePeriod.Should().Be(expiry.GracePeriod);
            entitlement.DeadlineUtc.Should().Be(expiry.DeadlineUtc);
            entitlement.IsExpired.Should().Be(expiry.IsExpired);
            entitlement.Overdue.Should().Be(expiry.Overdue);
            entitlement.Renewal.Should().Be(expiry.Renewal);
        }
    }
}
