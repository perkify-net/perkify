namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateBalanceWithZeroOrPositiveThreshold
        (
            [CombinatorialValues(0, 10)] long threshold
        )
        {
            var balance = new Balance(threshold);

            balance.Incoming.Should().Be(0);
            balance.Outgoing.Should().Be(0);
            balance.GetBalanceAmount().Should().Be(0);
            balance.Threshold.Should().Be(threshold);
            balance.GetBalanceType().Should().Be(BalanceType.Debit);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateBalanceWithNegativeThreshold
        (
            [CombinatorialValues(-10)] long threshold
        )
        {
            var balance = new Balance(threshold);

            balance.Incoming.Should().Be(0);
            balance.Outgoing.Should().Be(0);
            balance.GetBalanceAmount().Should().Be(0);
            balance.Threshold.Should().Be(threshold);
            balance.GetBalanceType().Should().Be(BalanceType.Credit);
        }

        [Fact(Skip = SkipOrNot)]
        public void TestCreateDebitBalanceDirectly()
        {
            var debit = Balance.Debit();

            debit.Incoming.Should().Be(0);
            debit.Outgoing.Should().Be(0);
            debit.GetBalanceAmount().Should().Be(0);
            debit.Threshold.Should().Be(0);
            debit.GetBalanceType().Should().Be(BalanceType.Debit);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateCreditBalanceDirectly
        (
            [CombinatorialValues(-10)] long threshold
        )
        {
            var credit = Balance.Credit(threshold);

            credit.Incoming.Should().Be(0);
            credit.Outgoing.Should().Be(0);
            credit.GetBalanceAmount().Should().Be(0);
            credit.Threshold.Should().Be(threshold);
            credit.GetBalanceType().Should().Be(BalanceType.Credit);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateCreditBalanceDirectlyWithInvalidThreshold
        (
            [CombinatorialValues(0, 10)] long threshold
        )
        {
            var parameter = nameof(threshold);
            var action = () => Balance.Credit(threshold);

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
