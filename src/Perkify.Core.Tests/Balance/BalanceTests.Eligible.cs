namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory]
        [InlineData(0, 100, 100, true)]
        [InlineData(0, 100, 50, true)]
        [InlineData(0, 50, 100, false)]
        [InlineData(-10, 100, 100, true)]
        [InlineData(-10, 100, 50, true)]
        [InlineData(-10, 50, 60, true)]
        [InlineData(-10, 50, 100, false)]
        public void TestIsEligible(long threshold, long incoming, long outgoing, bool eligible)
        {
            var balance = new Balance(threshold).Adjust(incoming, outgoing);
            Assert.Equal(threshold, balance.Threshold);
            Assert.Equal(eligible, balance.IsEligible);
        }
    }
}
