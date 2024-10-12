namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestTopupBalance
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(0L, 50L, 100L)] long incoming,
            [CombinatorialValues(0L, 50L, 100L)] long outgoing,
            [CombinatorialValues(0L, 10L)] long topup
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);

            balance.Topup(topup);
            var expected = incoming + topup;
            balance.Incoming.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestTopupBalanceInvalidAmount
        (
            [CombinatorialValues(0, -10)] long threshold,
            [CombinatorialValues(100)] long incoming,
            [CombinatorialValues(100)] long outgoing,
            [CombinatorialValues(-10)] long delta
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);

            var parameter = nameof(delta);
            var action = () => balance.Topup(delta);
            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Amount must be positive or zero. (Parameter '{parameter}')");
            balance.Incoming.Should().Be(incoming);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestTopupBalanceWithOverflowException
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(100L)] long outgoing,
            [CombinatorialValues(long.MaxValue - 9L)] long delta
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);

            var action = () => balance.Topup(delta);
            action
                .Should()
                .Throw<OverflowException>()
                .WithMessage($"Arithmetic operation resulted in an overflow.");
            Assert.Equal(incoming, balance.Incoming);
        }
    }
}
