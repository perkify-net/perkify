namespace Perkify.Core.Tests;

using BalanceStateChangeEventArgs = StateChangeEventArgs<BalanceState, BalanceStateOperation>;

public partial class BalanceTests
{
    [Theory, CombinatorialData]
    public void TestAdjustBalance
    (
        [CombinatorialValues(0L, -10L)] long threshold,
        [CombinatorialValues(0L, 100L, -100L)] long? amount,
        [CombinatorialValues(true, false)] bool isIncomingAdjusted,
        [CombinatorialValues(true, false)] bool isOutgoingAdjusted,
        [CombinatorialValues(true, false)] bool isStateChangedEventHooked
    )
    {
        var balance = new Balance(threshold).WithBalance(1000L, 500L);
        balance.Threshold.Should().Be(threshold);
        balance.Incoming.Should().Be(1000L);
        balance.Outgoing.Should().Be(500L);
        BalanceStateChangeEventArgs? stateChangedEvent = null;
        if (isStateChangedEventHooked)
        {
            balance.StateChanged += (sender, e) => { stateChangedEvent = e; };
        }

        var dic = isIncomingAdjusted ? amount : null;
        var dog = isOutgoingAdjusted ? amount : null;
        balance.Adjust(incoming: dic, outgoing: dog);

        var incoming = isIncomingAdjusted ? 1000L + dic : 1000L;
        var outgoing = isOutgoingAdjusted ? 500L + dog : 500L;
        balance.Incoming.Should().Be(incoming);
        balance.Outgoing.Should().Be(outgoing);
        if (isStateChangedEventHooked)
        {
            stateChangedEvent.Should().NotBeNull();
            stateChangedEvent!.Operation.Should().Be(BalanceStateOperation.Adjust);
            stateChangedEvent!.From.BalanceExceedancePolicy.Should().Be(BalanceExceedancePolicy.Reject);
            stateChangedEvent!.From.Threshold.Should().Be(threshold);
            stateChangedEvent!.From.Incoming.Should().Be(1000L);
            stateChangedEvent!.From.Outgoing.Should().Be(500L);
            stateChangedEvent!.To.BalanceExceedancePolicy.Should().Be(BalanceExceedancePolicy.Reject);
            stateChangedEvent!.To.Threshold.Should().Be(threshold);
            stateChangedEvent!.To.Incoming.Should().Be(incoming);
            stateChangedEvent!.To.Outgoing.Should().Be(outgoing);
        }
    }

    [Theory, CombinatorialData]
    public void TestClearBalance
    (
        [CombinatorialValues(0, -10)] long threshold,
        [CombinatorialValues(0, 50, 100)] long incoming,
        [CombinatorialValues(0, 50, 100)] long outgoing
    )
    {
        var balance = new Balance(threshold).WithBalance(incoming, outgoing);
        balance.Threshold.Should().Be(threshold);
        balance.Incoming.Should().Be(incoming);
        balance.Outgoing.Should().Be(outgoing);

        balance.Clear();
        balance.Incoming.Should().Be(0);
        balance.Outgoing.Should().Be(0);
    }
}
