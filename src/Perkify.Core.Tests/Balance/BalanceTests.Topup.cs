namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory]
        [InlineData(0, 100, 100, 10, 110, true)]        // Debit: Ineligible => Eligible    
        [InlineData(0, 100, 50, 10, 110, true)]         // Debit: Eligible => Eligible
        [InlineData(0, 50, 100, 10, 60, false)]         // Debit: Ineligible => Ineligible
        [InlineData(0, 50, 100, 50, 100, true)]         // Debit: Ineligible => Ineligible
        [InlineData(-10, 100, 100, 10, 110, true)]      // Credit: Ineligible => Eligible
        [InlineData(-10, 100, 50, 10, 110, true)]       // Credit: Eligible => Eligible
        [InlineData(-10, 50, 100, 10, 60, false)]       // Credit: Ineligible => Ineligible
        public void TestTopupBalance(long threshold, long incoming, long outgoing, long topup, long expected, bool eligible)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            balance.Topup(topup);
            Assert.Equal(expected, balance.Incoming);
            Assert.Equal(eligible, balance.IsEligible);
        }

        [Theory]
        [InlineData(0, 100, 100, -10)]
        [InlineData(-10, 100, 100, -10)]
        public void TestTopupBalanceNegativeAmount(long threshold, long incoming, long outgoing, long topup)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Topup(topup));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 100, long.MaxValue - 9)]
        [InlineData(-10, 100, 100, long.MaxValue - 9)]
        public void TestTopupBalanceOverflow(long threshold, long incoming, long outgoing, long topup)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<OverflowException>(() => balance.Topup(topup));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }
    }
}
