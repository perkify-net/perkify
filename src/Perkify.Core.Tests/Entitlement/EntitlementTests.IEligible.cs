namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementTests
    {
        [Theory, CombinatorialData]
        public void TestIsEligible
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(true, false)] bool isBalanceEligible,
            [CombinatorialValues(true, false)] bool isExpryEligible,
            [CombinatorialValues(true, false)] bool isEnablementEligible,
            [CombinatorialValues(true, false)] bool iskPrerequesiteEligible
        )
        {
            var mockBalance = new Mock<Balance>(MockBehavior.Strict, 0L, BalanceExceedancePolicy.Reject);
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            mockBalance.SetupGet(balance => balance.IsEligible).Returns(isBalanceEligible);
            var mockExpiry = new Mock<Expiry>(MockBehavior.Strict, DateTime.UtcNow, clock);
            mockExpiry.SetupGet(expiry => expiry.IsEligible).Returns(isExpryEligible);
            var mockEnablement = new Mock<Enablement>(MockBehavior.Strict, true, clock);
            mockEnablement.SetupGet(enablement => enablement.IsEligible).Returns(isEnablementEligible);
            var mockPrerequesite = new Mock<IEligible>();
            mockPrerequesite.SetupGet(eligible =>eligible.IsEligible).Returns(iskPrerequesiteEligible);
            var entitlement = new Entitlement(AutoRenewalMode.None)
            {
                Balance = mockBalance.Object,
                Expiry = mockExpiry.Object,
                Enablement = mockEnablement.Object,
                Prerequesite = mockPrerequesite.Object,
            };

            var expected = new[] { isBalanceEligible, isExpryEligible, isEnablementEligible, iskPrerequesiteEligible }.All(eligible => eligible);
            entitlement.IsEligible.Should().Be(expected);
        }

        [Fact]
        public void TestIsEligibleNotInitialized()
        {
            var entitlement = new Entitlement(AutoRenewalMode.None);
            entitlement.IsEligible.Should().BeTrue();
        }
    }
}
