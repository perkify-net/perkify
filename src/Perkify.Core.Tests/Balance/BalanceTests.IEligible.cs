namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory, CombinatorialData]
        public void TestIsEligible
        (
            [CombinatorialValues(0, -10)] long threshold,
            [CombinatorialValues(10)] long incoming,
            [CombinatorialValues(0, 10, -10)] long delta
        )
        {
            var outgoing = incoming - threshold - delta;
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);

            var eligible = delta >= 0;
            balance.IsEligible.Should().Be(eligible);
        }
    }
}
