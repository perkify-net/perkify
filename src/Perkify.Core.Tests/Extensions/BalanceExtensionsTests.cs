namespace Perkify.Core.Tests
{
    /*
    public class BalanceExtensionsTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(10L, BalanceType.Debit)]
        [InlineData(0L, BalanceType.Debit)]
        [InlineData(-10L, BalanceType.Credit)]
        public void TestGetBalanceType(long threshold, BalanceType expected)
        {
            var mock = new Mock<IBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            var balance = mock.Object;

            var actual = balance.BalanceType;
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 10L)]
        [InlineData(-10L, 100L, 90L, 10L)]
        [InlineData(0L, 100L, 120L, -20L)]
        [InlineData(-10L, 100L, 120L, -20L)]
        public void TestGetBalanceAmount(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.Gross;
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 0L)]
        [InlineData(-10L, 100L, 90L, 0L)]
        [InlineData(0L, 100L, 120L, 20L)]
        [InlineData(-10L, 100L, 120L, 10L)]
        public void TestGetOverspendingAmount(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.Overspending;
            actual.Should().Be(expected);
        }
    }
    */
}
