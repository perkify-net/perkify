namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementChainTests
    {
        [Theory]
        [InlineData("2024-10-09T15:00:00Z", true, true, true)]
        [InlineData("2024-10-09T15:00:00Z", true, false, true)]
        [InlineData("2024-10-09T15:00:00Z", false, true, true)]
        [InlineData("2024-10-09T15:00:00Z", false, false, false)]
        public void TestIsEligible(string nowUtcString, bool isEligibleX, bool isEligibleY, bool expected)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(isEligibleX),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(isEligibleY),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            chain.IsEligible.Should().Be(expected);
        }

        [Fact]
        public void TestIsEligibleWithEmptyEntitlement()
        {
            var chain = new EntitlementChain(null);
            chain.Entitlements.Should().HaveCount(0);
            chain.IsEligible.Should().BeFalse();
        }
    }
}
