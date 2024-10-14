namespace Perkify.Core.Tests
{
    public class DelegationTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(false)]
        [InlineData(true)]
        public void TestDelegation(bool eligible)
        {
            var delegation = new Delegation(() => eligible);
            delegation.IsEligible.Should().Be(eligible);
        }

        [Fact(Skip = SkipOrNot)]
        public void TestCheckEligible()
        {
            var delgation = new Delegation(() => true) as IEligible;
            var action = new Action(() => (delgation as IEligible).Check());
            action.Should().NotThrow();
        }

        [Fact(Skip = SkipOrNot)]
        public void TestCheckIneligible()
        {
            var delgation = new Delegation(() => false) as IEligible;
            var action = new Action(() => (delgation as IEligible).Check());
            action.Should().Throw<InvalidOperationException>().WithMessage("Ineligible state.");
        }
    }
}
