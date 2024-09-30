namespace Perkify.Core.Tests
{
    public class BalanceExceedancePolicyTests
    {
        [Theory]
        [InlineData(BalanceExceedancePolicy.Reject, 10, 100, 10, 0)]
        [InlineData(BalanceExceedancePolicy.Overflow, 10, 100, 10, 0)]
        [InlineData(BalanceExceedancePolicy.Overdraft, 10, 100, 10, 0)]
        public void TestDeduct(BalanceExceedancePolicy policy, long input, long maximum, long output, long expected)
        {
            var delta = input;
            var actual = policy.Deduct(ref delta, maximum);
            Assert.Equal(output, delta);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(BalanceExceedancePolicy.Overflow, 110, 100, 100, 10)]
        [InlineData(BalanceExceedancePolicy.Overdraft, 110, 100, 110, 0)]
        public void TestDeductWithExceedance(BalanceExceedancePolicy policy, long input, long maximum, long output, long expected)
        {
            var delta = input;
            var actual = policy.Deduct(ref delta, maximum);
            Assert.Equal(output, delta);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(BalanceExceedancePolicy.Reject, 110, 100, 110)]
        public void TestDeductWithExceedanceRejected(BalanceExceedancePolicy policy, long input, long maximum, long output)
        {
            var delta = input;
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.Deduct(ref delta, maximum));
            Assert.Equal(output, delta);
        }
    }
}
