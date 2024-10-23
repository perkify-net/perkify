namespace Perkify.Core.Tests
{
    using BalanceStateChangeEventArgs = StateChangeEventArgs<BalanceState, BalanceStateOperation>;

    public partial class BalanceTests
    {
        [Theory, CombinatorialData]
        public void TestTopupBalance
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(0L, 50L, 100L)] long incoming,
            [CombinatorialValues(0L, 50L, 100L)] long outgoing,
            [CombinatorialValues(0L, 10L)] long topup,
            [CombinatorialValues(true, false)] bool isStateChangedEventHooked
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            BalanceStateChangeEventArgs? stateChangedEvent = null;
            if (isStateChangedEventHooked)
            {
                balance.StateChanged += (sender, e) => { stateChangedEvent = e; };
            }

            balance.Topup(topup);
            var expected = incoming + topup;
            balance.Incoming.Should().Be(expected);
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
                stateChangedEvent!.To.Incoming.Should().Be(expected);
                stateChangedEvent!.To.Outgoing.Should().Be(outgoing);
            }
        }

        [Theory, CombinatorialData]
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

        [Theory, CombinatorialData]
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
