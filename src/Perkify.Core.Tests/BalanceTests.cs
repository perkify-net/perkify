namespace Perkify.Core.UnitTests
{
    using Perkify.Core;

    public class BalanceTests
    {
        #region Create Balance Tests

        [Theory]
        [InlineData(10, BalanceType.Debit)]
        [InlineData(0, BalanceType.Debit)]
        [InlineData(-10, BalanceType.Credit)]
        public void TestCreateBalance(long threshold, BalanceType type)
        {
            var balance = new Balance(threshold);
            Assert.Equal(0, balance.Incoming);
            Assert.Equal(0, balance.Outgoing);
            Assert.Equal(0, balance.GetBalanceAmount());
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(type, balance.GetBalanceType());
        }

        [Fact]
        public void TestCreateDebitBalance()
        {
            var debit = Balance.Debit();
            Assert.Equal(0, debit.Incoming);
            Assert.Equal(0, debit.Outgoing);
            Assert.Equal(0, debit.GetBalanceAmount());
            Assert.Equal(0, debit.Threshold);
            Assert.Equal(BalanceType.Debit, debit.GetBalanceType());
        }

        [Theory]
        [InlineData(-10)]
        public void TestCreateCreditBalance(long threshold)
        {
            var credit = Balance.Credit(threshold);
            Assert.Equal(0, credit.Incoming);
            Assert.Equal(0, credit.Outgoing);
            Assert.Equal(0, credit.GetBalanceAmount());
            Assert.Equal(threshold, credit.Threshold);
            Assert.Equal(BalanceType.Credit, credit.GetBalanceType());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void TestCreateCreditBalancedInvalidThreshold(long threshold)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Balance.Credit(threshold));
        }

        #endregion

        #region Adjust & Clear Balance Tests

        [Theory]
        [InlineData(0, 100, 100)]
        [InlineData(0, 100, 50)]
        [InlineData(0, 50, 100)]
        [InlineData(-10, 100, 100)]
        [InlineData(-10, 100, 50)]
        [InlineData(-10, 50, 100)]
        public void TestAdjustBalance(long threshold, long incoming, long outgoing)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(0, 50)]
        [InlineData(0, 0)]
        [InlineData(-10, 100)]
        [InlineData(-10, 50)]
        [InlineData(-10, 0)]
        public void TestAdjustBalanceIncomingOnly(long threshold, long incoming)
        {
            var balance = new Balance(threshold).Adjust(100, 50);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(100, balance.Incoming);
            Assert.Equal(50, balance.Outgoing);
            balance.Adjust(incoming, null);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(50, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(0, 50)]
        [InlineData(0, 0)]
        [InlineData(-10, 100)]
        [InlineData(-10, 50)]
        [InlineData(-10, 0)]
        public void TestAdjustBalanceOutgoingOnly(long threshold, long outgoing)
        {
            var balance = new Balance(threshold).Adjust(100, 50);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(100, balance.Incoming);
            Assert.Equal(50, balance.Outgoing);
            balance.Adjust(null, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(100, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, -100, -100)]
        [InlineData(0, -100, 100)]
        [InlineData(0, 100, -100)]
        [InlineData(-10, -100, -100)]
        [InlineData(-10, -100, 100)]
        [InlineData(-10, 100, -100)]
        public void TestAdjustBalanceAmountOutofRange(long threshold, long incoming, long outgoing)
        {
            var balance = new Balance(threshold);
            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Adjust(incoming, outgoing));
        }

        [Theory]
        [InlineData(0, 100, 100)]
        [InlineData(0, 100, 50)]
        [InlineData(0, 50, 100)]
        [InlineData(-10, 100, 100)]
        [InlineData(-10, 100, 50)]
        [InlineData(-10, 50, 100)]
        public void TestClearBalance(long threshold, long incoming, long outgoing)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing).Clear();
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(0L, balance.Incoming);
            Assert.Equal(0L, balance.Outgoing);
        }

        #endregion

        #region IEligible Tests

        [Theory]
        [InlineData(0, 100, 100, true)]
        [InlineData(0, 100, 50, true)]
        [InlineData(0, 50, 100, false)]
        [InlineData(-10, 100, 100, true)]
        [InlineData(-10, 100, 50, true)]
        [InlineData(-10, 50, 60, true)]
        [InlineData(-10, 50, 100, false)]
        public void TestBalanceEligible(long threshold, long incoming, long outgoing, bool eligible)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(eligible, balance.IsEligible);
        }

        #endregion

        #region Topup & Spend Balance Tests

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

        [Theory]
        [InlineData(0, 100, 50, 10, 60)]                // Debit: Eligible => Eligible
        [InlineData(0, 100, 100, 10, 110)]              // Debit: Eligible => Eligible
        [InlineData(0, 100, 50, 50, 100)]               // Debit: Eligible => Ineligible
        [InlineData(-10, 100, 100, 10, 110)]            // Credit: Ineligible => Eligible
        [InlineData(-10, 100, 50, 10, 60)]              // Credit: Eligible => Eligible
        public void TestSpendBalance(long threshold, long incoming, long outgoing, long spend, long expected)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            balance.Spend(spend, overspending: true);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(expected, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 100, -10, true)]
        [InlineData(0, 100, 100, -10, false)]
        [InlineData(-10, 100, 100, -10, true)]
        [InlineData(-10, 100, 100, -10, false)]
        public void TestSpendBalanceNegativeAmount(long threshold, long incoming, long outgoing, long spend, bool overspending)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Spend(spend, overspending));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 50, 100, 10, true, 50)]
        [InlineData(0, 50, 100, 10, false, 50)]
        [InlineData(-10, 50, 100, 10, true, 40)]
        [InlineData(-10, 50, 100, 10, false, 40)]
        public void TestSpendBalanceIneligible(long threshold, long incoming, long outgoing, long spend, bool overspending, long expected)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.False(balance.IsEligible);

            Assert.Throws<InvalidOperationException>(() => balance.Spend(spend, overspending));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
            Assert.Equal(expected, balance.GetOverSpendingAmount());
        }

        [Theory]
        [InlineData(0, 100, 50, 60)]
        [InlineData(-10, 0, 0, 20)]
        public void TestSpendBalanceOverspending(long threshold, long incoming, long outgoing, long spend)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<ArgumentOutOfRangeException>(() => balance.Spend(spend, false));
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory]
        [InlineData(0, 100, 100, long.MaxValue - 9)]
        [InlineData(-10, 100, 100, long.MaxValue - 9)]
        public void TestSpendBalanceOverflow(long threshold, long incoming, long outgoing, long spend)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(incoming, balance.Incoming);
            Assert.Equal(outgoing, balance.Outgoing);

            Assert.Throws<OverflowException>(() => balance.Spend(spend, overspending: true));
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

        #endregion
    }
}
