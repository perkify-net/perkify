namespace Perkify.Core.Tests
{
    public class BalanceExtensionsTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(10L, BalanceType.Debit)]
        [InlineData(0L, BalanceType.Debit)]
        [InlineData(-10L, BalanceType.Credit)]
        public void TestGetBalanceType(long threshold, BalanceType expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            var balance = mock.Object;

            var actual = balance.GetBalanceType();
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 10L)]
        [InlineData(-10L, 100L, 90L, 10L)]
        [InlineData(0L, 100L, 120L, -20L)]
        [InlineData(-10L, 100L, 120L, -20L)]
        public void TestGetBalanceAmount(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.GetBalanceAmount();
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 0L)]
        [InlineData(-10L, 100L, 90L, 0L)]
        [InlineData(0L, 100L, 120L, 20L)]
        [InlineData(-10L, 100L, 120L, 10L)]
        public void TestGetOverspendingAmount(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.GetOverspendingAmount();
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 10L)]
        [InlineData(0L, 100L, 100L, 0L)]
        [InlineData(0L, 100L, 110L, 0L)]
        [InlineData(-10L, 100L, 90L, 20L)]
        [InlineData(-10L, 100L, 110L, 0L)]
        [InlineData(-10L, 100L, 120L, 0L)]
        public void TestGetMaxDeductibleAmountWithRejectPolicy
        (long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.GetMaxDeductibleAmount(BalanceExceedancePolicy.Reject);
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, 10L)]
        [InlineData(0L, 100L, 100L, 0L)]
        [InlineData(0L, 100L, 110L, 0L)]
        [InlineData(-10L, 100L, 90L, 20L)]
        [InlineData(-10L, 100L, 110L, 0L)]
        [InlineData(-10L, 100L, 120L, 0L)]
        public void TestGetMaxDeductibleAmountWithOverflowPolicy(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.GetMaxDeductibleAmount(BalanceExceedancePolicy.Overflow);
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0L, 100L, 90L, long.MaxValue)]
        [InlineData(0L, 100L, 100L, long.MaxValue)]
        [InlineData(0L, 100L, 110L, long.MaxValue)]
        [InlineData(-10L, 100L, 90L, long.MaxValue)]
        [InlineData(-10L, 100L, 110L, long.MaxValue)]
        [InlineData(-10L, 100L, 120L, long.MaxValue)]
        public void TestGetMaxDeductibleAmountWithOverdraftPolicy(long threshold, long incoming, long outgoing, long expected)
        {
            var mock = new Mock<IMockBalance>();
            mock.SetupGet(x => x.Threshold).Returns(threshold);
            mock.SetupGet(x => x.Incoming).Returns(incoming);
            mock.SetupGet(x => x.Outgoing).Returns(outgoing);
            var balance = mock.Object;

            var actual = balance.GetMaxDeductibleAmount(BalanceExceedancePolicy.Overdraft);
            actual.Should().Be(expected);
        }
    }
}
