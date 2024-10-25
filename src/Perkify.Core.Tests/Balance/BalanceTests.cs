namespace Perkify.Core.Tests;

public partial class BalanceTests
{
    [Theory, CombinatorialData]
    public void TestCreateBalance
    (
        [CombinatorialValues(-10L, 0L, 10L)] long threshold,
        [CombinatorialValues
        (
            BalanceExceedancePolicy.Reject, 
            BalanceExceedancePolicy.Overflow,
            BalanceExceedancePolicy.Overdraft
        )] BalanceExceedancePolicy policy
    )
    {
        var balance = new Balance(threshold, policy);

        balance.Incoming.Should().Be(0);
        balance.Outgoing.Should().Be(0);
        balance.Gross.Should().Be(0);
        balance.Threshold.Should().Be(threshold);
        balance.BalanceExceedancePolicy.Should().Be(policy);
    }

    [Theory]
    [InlineData(BalanceExceedancePolicy.Reject)]
    [InlineData(BalanceExceedancePolicy.Overflow)]
    [InlineData(BalanceExceedancePolicy.Overdraft)]
    public void TestCreateDebitBalanceDirectly(BalanceExceedancePolicy policy)
    {
        var debit = Balance.Debit(policy);
        debit.Threshold.Should().Be(0L);
        debit.BalanceType.Should().Be(BalanceType.Debit);
        debit.Incoming.Should().Be(0L);
        debit.Outgoing.Should().Be(0L);
        debit.Gross.Should().Be(0L);
        debit.Available.Should().Be(0L);
        debit.BalanceExceedancePolicy.Should().Be(policy);
    }

    [Theory, CombinatorialData]
    public void TestCreateCreditBalanceDirectly
    (
        [CombinatorialValues(-10L)] long threshold,
        [CombinatorialValues
        (
            BalanceExceedancePolicy.Reject,
            BalanceExceedancePolicy.Overflow,
            BalanceExceedancePolicy.Overdraft
        )] BalanceExceedancePolicy policy
    )
    {
        var credit = Balance.Credit(threshold, policy);
        credit.Threshold.Should().Be(threshold);
        credit.Threshold.Should().BeNegative();
        credit.BalanceType.Should().Be(BalanceType.Credit);
        credit.Incoming.Should().Be(0L);
        credit.Outgoing.Should().Be(0L);
        credit.Gross.Should().Be(0L);
        credit.Available.Should().BePositive();
        credit.Available.Should().Be(-threshold);
        credit.BalanceExceedancePolicy.Should().Be(policy);
    }

    [Theory, CombinatorialData]
    public void TestCreateCreditBalanceDirectlyWithInvalidThreshold
    (
        [CombinatorialValues(0, 10)] long threshold,
        [CombinatorialValues
        (
            BalanceExceedancePolicy.Reject,
            BalanceExceedancePolicy.Overflow,
            BalanceExceedancePolicy.Overdraft
        )] BalanceExceedancePolicy policy
    )
    {
        var parameter = nameof(threshold);
        var action = () => Balance.Credit(threshold, policy);

        action
            .Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName(parameter)
            .WithMessage($"Amount must be negative (Parameter '{parameter}')");
    }

    [Theory, CombinatorialData]
    public void TestWithBalance
    (
        [CombinatorialValues(0, 100, 200)] long incoming,
        [CombinatorialValues(0, 100, 200)] long outgoing
    )
    {
        var balance = Balance.Debit().WithBalance(incoming, outgoing);
        balance.Incoming.Should().Be(incoming);
        balance.Outgoing.Should().Be(outgoing);
    }

    [Theory, CombinatorialData]
    public void TestWithBalanceInNegativeAmount
    (
        [CombinatorialValues(-100)] long amount,
        [CombinatorialValues(null, true, false)] bool? incomingOrOutgoing
    )
    {
        var isIncomingNegative = incomingOrOutgoing ?? true;
        var isOutgoingNegative = !(incomingOrOutgoing ?? false);
        var incoming = isIncomingNegative ? amount : 0L;
        var outgoing = isOutgoingNegative ? amount : 0L;
        var parameter = isIncomingNegative ? "incoming" : "outgoing";
        var balance = Balance.Debit();
        var action = () => balance.WithBalance(incoming, outgoing);

        _ = action
            .Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName(parameter)
            .WithMessage($"Amount must be positive or zero. (Parameter '{parameter}')");
        balance.Incoming.Should().Be(0);
        balance.Outgoing.Should().Be(0);
    }
}
