using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Perkify.Core.Tests
{
    public partial class BalanceTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalance
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(50L, 90L)] long outgoing,
            [CombinatorialValues(0L, 10L)] long delta,
            [CombinatorialValues
            (
                BalanceExceedancePolicy.Overdraft,
                BalanceExceedancePolicy.Overflow,
                BalanceExceedancePolicy.Reject
            )] BalanceExceedancePolicy policy
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);

            delta = incoming - threshold - outgoing - delta;
            var remained = balance.Deduct(delta, policy);
            var expected = outgoing + delta;
            balance.Outgoing.Should().Be(expected);
            remained.Should().Be(0);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalanceNegativeAmount
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(100L)] long outgoing,
            [CombinatorialValues(-10L)] long delta,
            [CombinatorialValues
            (
                BalanceExceedancePolicy.Overdraft,
                BalanceExceedancePolicy.Overflow,
                BalanceExceedancePolicy.Reject
            )] BalanceExceedancePolicy policy
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeTrue();

            var parameter = nameof(delta);
            var action = () => balance.Deduct(delta, policy);
            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Amount must be positive or zero. (Parameter '{parameter}')");
            balance.IsEligible.Should().BeTrue();
            balance.Outgoing.Should().Be(outgoing);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalanceIneligible
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(50L)] long incoming,
            [CombinatorialValues(100L)] long outgoing,
            [CombinatorialValues(10L)] long delta,
            [CombinatorialValues
            (
                BalanceExceedancePolicy.Overdraft,
                BalanceExceedancePolicy.Overflow,
                BalanceExceedancePolicy.Reject
            )] BalanceExceedancePolicy policy
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeFalse();

            var action = () => balance.Deduct(delta, policy);
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Ineligible state.");
            balance.IsEligible.Should().BeFalse();
            Assert.Equal(outgoing, balance.Outgoing);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(0, 100, 50, 60)]
        [InlineData(-10, 0, 0, 20)]
        public void TestDeductBalanceRejected
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(90L)] long outgoing,
            [CombinatorialValues(20L)] long exceed
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeTrue();

            var maximum = incoming - threshold - outgoing;
            var delta = maximum + exceed;
            var action = () => balance.Deduct(delta, BalanceExceedancePolicy.Reject);
            var parameter = nameof(delta);
            action
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(parameter)
                .WithMessage($"Rejected due to insufficient balance. (Parameter '{parameter}')");
            balance.IsEligible.Should().BeTrue();
            balance.Outgoing.Should().Be(outgoing);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalanceOverflow
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(90L)] long outgoing,
            [CombinatorialValues(20L)] long exceed
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeTrue();

            var maximum = incoming - threshold - outgoing;
            var delta = maximum + exceed;
            var remained = balance.Deduct(delta, BalanceExceedancePolicy.Overflow);
            var expected = outgoing + delta - remained;
            remained.Should().Be(exceed);
            balance.Outgoing.Should().Be(expected);
            balance.IsEligible.Should().BeTrue();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalanceOverdraft
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(90L)] long outgoing,
            [CombinatorialValues(20L)] long exceed
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeTrue();

            var maximum = incoming - threshold - outgoing;
            var delta = maximum + exceed;
            var remained = balance.Deduct(delta, BalanceExceedancePolicy.Overdraft);
            var expected = outgoing + delta - remained;
            remained.Should().Be(0);
            balance.Outgoing.Should().Be(expected);
            balance.IsEligible.Should().BeFalse();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeductBalanceOverdraftedWithOverflowException
        (
            [CombinatorialValues(0L, -10L)] long threshold,
            [CombinatorialValues(100L)] long incoming,
            [CombinatorialValues(100L)] long outgoing,
            [CombinatorialValues(long.MaxValue - 9)] long delta
        )
        {
            var balance = new Balance(threshold).WithBalance(incoming, outgoing);
            balance.Threshold.Should().Be(threshold);
            balance.Incoming.Should().Be(incoming);
            balance.Outgoing.Should().Be(outgoing);
            balance.IsEligible.Should().BeTrue();

            var action = () => balance.Deduct(delta, BalanceExceedancePolicy.Overdraft);
            action
                .Should()
                .Throw<OverflowException>()
                .WithMessage("Arithmetic operation resulted in an overflow.");
            balance.IsEligible.Should().BeTrue();
            balance.Outgoing.Should().Be(outgoing);
        }
    }
}
