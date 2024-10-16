namespace Perkify.Core.Tests
{
    using FluentAssertions;
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
            entitlement.NowUtc.Should().Be(nowUtc);
            entitlement.Incoming.Should().Be(balance.Incoming);
            entitlement.Outgoing.Should().Be(balance.Outgoing);
            entitlement.Threshold.Should().Be(balance.Threshold);
            entitlement.BalanceType.Should().Be(balance.BalanceType);
            entitlement.Overspending.Should().Be(balance.Overspending);
        }
    }
}
