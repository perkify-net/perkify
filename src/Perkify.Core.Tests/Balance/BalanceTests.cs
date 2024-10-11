namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        public void TestCreateBalanceWithZeroOrPositiveThreshold(long threshold)
        {
            var balance = new Balance(threshold);

            balance.Incoming.Should().Be(0);
            balance.Outgoing.Should().Be(0);
            balance.GetBalanceAmount().Should().Be(0);
            balance.Threshold.Should().Be(threshold);
            balance.GetBalanceType().Should().Be(BalanceType.Debit);
        }

        [Theory]
        [InlineData(-10)]
        public void TestCreateBalanceWithNegativeThreshold(long threshold)
        {
            var balance = new Balance(threshold);

            balance.Incoming.Should().Be(0);
            balance.Outgoing.Should().Be(0);
            balance.GetBalanceAmount().Should().Be(0);
            balance.Threshold.Should().Be(threshold);
            balance.GetBalanceType().Should().Be(BalanceType.Credit);
        }

        [Fact]
        public void TestCreateDebitBalanceDirectly()
        {
            var debit = Balance.Debit();
            debit.Incoming.Should().Be(0);
            debit.Outgoing.Should().Be(0);
            debit.GetBalanceAmount().Should().Be(0);
            debit.Threshold.Should().Be(0);
            debit.GetBalanceType().Should().Be(BalanceType.Debit);
        }

        [Theory]
        [InlineData(-10)]
        public void TestCreateCreditBalanceDirectly(long threshold)
        {
            var credit = Balance.Credit(threshold);
            credit.Incoming.Should().Be(0);
            credit.Outgoing.Should().Be(0);
            credit.GetBalanceAmount().Should().Be(0);
            credit.Threshold.Should().Be(threshold);
            credit.GetBalanceType().Should().Be(BalanceType.Credit);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void TestCreateCreditBalanceDirectlyWithInvalidThreshold(long threshold)
        {
            var parameter = nameof(threshold);
            var action = () => Balance.Credit(threshold);

            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Threshold amount must be negative (Parameter '{parameter}')");
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

            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Amount must be zero or positive (Parameter '{parameter}')");
            balance.Incoming.Should().Be(0);
            balance.Outgoing.Should().Be(0);
        }
    }
}
