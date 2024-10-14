namespace Perkify.Core.Tests
{
    public class BalanceExceedancePolicyExtensionsTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(BalanceExceedancePolicy.Reject, 10, 100, 10)]
        [InlineData(BalanceExceedancePolicy.Overflow, 10, 100, 10)]
        [InlineData(BalanceExceedancePolicy.Overdraft, 10, 100, 10)]

        public void TestDeductWithoutExceedance
        (
            BalanceExceedancePolicy policy,
            long input, long maximum, long output
        )
        {
            var delta = input;
            var actual = policy.Deduct(ref delta, maximum);
            delta.Should().Be(output);
            actual.Should().Be(0);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(110, 100, 110)]
        public void TestDeductWithExceedanceRejected(long input, long maximum, long output)
        {
            var delta = input;
            var action = () => BalanceExceedancePolicy.Reject.Deduct(ref delta, maximum);
            var parameter = nameof(delta);

            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Rejected due to insufficient balance. (Parameter '{parameter}')");
            delta.Should().Be(output);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(110L, 100L, 100L, 10L)]
        public void TestDeductWithExceedanceOverflowed(long input, long maximum, long output, long expected)
        {
            var delta = input;
            var actual = BalanceExceedancePolicy.Overflow.Deduct(ref delta, maximum);
            delta.Should().Be(output);
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(110, 100, 110)]
        public void TestDeductWithExceedanceOverdrafted(long input, long maximum, long output)
        {
            var delta = input;
            var actual = BalanceExceedancePolicy.Overdraft.Deduct(ref delta, maximum);
            delta.Should().Be(output);
            actual.Should().Be(0L);
        }

        [Fact(Skip = SkipOrNot)]
        public void TestDeductWithUnsupportedPolicy()
        {
            var delta = 110L;
            var policy = (BalanceExceedancePolicy)999;

            var action = () => policy.Deduct(ref delta, 100);
            action
                .Should()
                .Throw<NotSupportedException>()
                .WithMessage($"Unsupported balance exceedance policy: {policy}");
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestGetDeductibleAllowance
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(20L, 10L, 0L, -10L)] long delta,
            [CombinatorialValues
            (
                BalanceExceedancePolicy.Reject, 
                BalanceExceedancePolicy.Overflow
            )] BalanceExceedancePolicy policy
        )
        {
            var gross = threshold + delta;
            var expected = delta > 0L ? delta : 0L;
            var actual = policy.GetDeductibleAllowance(gross, threshold);
            actual.Should().Be(expected);
            actual.Should().BeGreaterThanOrEqualTo(0L);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestGetDeductibleAllowanceUnlimited
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(20L, 10L, 0L, -10L)] long delta,
            [CombinatorialValues(BalanceExceedancePolicy.Overdraft)] BalanceExceedancePolicy policy
        )
        {
            var gross = threshold + delta;
            var actual = policy.GetDeductibleAllowance(gross, threshold);
            actual.Should().Be(long.MaxValue);
        }

        [Fact(Skip = SkipOrNot)]
        public void TestGetDeductibleAllowanceWithUnsupportedPolicy()
        {
            var policy = (BalanceExceedancePolicy)999;

            var action = () => policy.GetDeductibleAllowance(100, 0);
            action
                .Should()
                .Throw<NotSupportedException>()
                .WithMessage($"Unsupported balance exceedance policy: {policy}");
        }
    }
}
