namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory]
        [InlineData(0, 100, 50, 10, 60)]                // Debit: Eligible => Eligible
        [InlineData(0, 100, 100, 10, 110)]              // Debit: Eligible => Eligible
        [InlineData(0, 100, 50, 50, 100)]               // Debit: Eligible => Ineligible
        [InlineData(-10, 100, 100, 10, 110)]            // Credit: Ineligible => Eligible
        [InlineData(-10, 100, 50, 10, 60)]              // Credit: Eligible => Eligible
        public void TestDeductBalance(long threshold, long incoming, long outgoing, long spend, long expected)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            balance.Deduct(spend, BalanceExceedancePolicy.Overdraft);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(expected, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 100, -10, BalanceExceedancePolicy.Overdraft)]
        [InlineData(0, 100, 100, -10, BalanceExceedancePolicy.Reject)]
        [InlineData(-10, 100, 100, -10, BalanceExceedancePolicy.Overdraft)]
        [InlineData(-10, 100, 100, -10, BalanceExceedancePolicy.Reject)]
        public void TestDeductBalanceNegativeAmount(long threshold, long incoming, long outgoing, long spend, BalanceExceedancePolicy policy)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Deduct(spend, policy));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 50, 100, 10, BalanceExceedancePolicy.Overdraft, 50)]
        [InlineData(0, 50, 100, 10, BalanceExceedancePolicy.Reject, 50)]
        [InlineData(-10, 50, 100, 10, BalanceExceedancePolicy.Overdraft, 40)]
        [InlineData(-10, 50, 100, 10, BalanceExceedancePolicy.Reject, 40)]
        public void TestDeductBalanceIneligible(long threshold, long incoming, long outgoing, long spend, BalanceExceedancePolicy policy, long expected)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.False(balance.IsEligible);

            Assert.Throws<InvalidOperationException>(() => balance.Deduct(spend, policy));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.Equal(expected, balance.GetOverSpendingAmount());
        }

        [Theory]
        [InlineData(0, 100, 50, 60)]
        [InlineData(-10, 0, 0, 20)]
        public void TestDeductBalanceOverspending(long threshold, long incoming, long outgoing, long spend)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Deduct(spend, BalanceExceedancePolicy.Reject));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 100, long.MaxValue - 9)]
        [InlineData(-10, 100, 100, long.MaxValue - 9)]
        public void TestDeductBalanceOverflow(long threshold, long incoming, long outgoing, long spend)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<OverflowException>(() => balance.Deduct(spend, BalanceExceedancePolicy.Overdraft));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 50, 0)]         // Debit: without overspending
        [InlineData(0, 100, 100, 0)]        // Debit: without overspending (on the edge)
        [InlineData(0, 100, 120, 20)]       // Debit: with overspending
        [InlineData(-10, 100, 105, 0)]      // Credit: without overspending
        [InlineData(-10, 100, 110, 0)]      // Credit: without overspending (on the edge)
        [InlineData(-10, 100, 120, 10)]     // Credit: with overspending
        public void TestBalanceOverspending(long threshold, long incoming, long outgoing, long overspending)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.Equal(overspending, balance.GetOverSpendingAmount());
        }
    }
}
