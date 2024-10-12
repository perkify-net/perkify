namespace Perkify.Core.Tests
{

    public partial class BalanceExtensionsTests
    {
        const string SkipOrNot = null;

        /*
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestBalanceOverspending
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(90L)] long outgoing,
            [CombinatorialValues(20L)] long exceed
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            var maximum = incoming - threshold - outgoing;
            var delta = maximum + exceed;
            var remained = balance.Deduct(delta, BalanceExceedancePolicy.Overdraft);
            remained.Should().Be(0);
            balance.IsEligible.Should().BeFalse();
            balance.GetOverSpendingAmount().Should().Be(exceed);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestBalanceOverspendingZero
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L, 200L)] long incoming,
            [CombinatorialValues(10L, 20L)] long outgoing
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.IsEligible.Should().BeTrue();
            balance.GetOverSpendingAmount().Should().Be(0);
        }
        */
    }
}
