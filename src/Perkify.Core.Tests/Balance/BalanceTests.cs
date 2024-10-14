namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
            balance.Policy.Should().Be(policy);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(BalanceExceedancePolicy.Reject)]
        [InlineData(BalanceExceedancePolicy.Overflow)]
        [InlineData(BalanceExceedancePolicy.Overdraft)]
        public void TestCreateDebitBalanceDirectly(BalanceExceedancePolicy policy)
        {
            var debit = Balance.Debit(policy);

            debit.Incoming.Should().Be(0);
            debit.Outgoing.Should().Be(0);
            debit.Gross.Should().Be(0);
            debit.Threshold.Should().Be(0);
            debit.Policy.Should().Be(policy);
            debit.BalanceType.Should().Be(BalanceType.Debit);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateCreditBalanceDirectly
        (
            [CombinatorialValues(-10)] long threshold,
            [CombinatorialValues
            (
                BalanceExceedancePolicy.Reject,
                BalanceExceedancePolicy.Overflow,
                BalanceExceedancePolicy.Overdraft
            )] BalanceExceedancePolicy policy
        )
        {
            var credit = Balance.Credit(threshold, policy);

            credit.Incoming.Should().Be(0);
            credit.Outgoing.Should().Be(0);
            credit.Gross.Should().Be(0);
            credit.Threshold.Should().Be(threshold);
            credit.Threshold.Should().BeNegative();
            credit.Policy.Should().Be(policy);
            credit.BalanceType.Should().Be(BalanceType.Credit);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
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
}
