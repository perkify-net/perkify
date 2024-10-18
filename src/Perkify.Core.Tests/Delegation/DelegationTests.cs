namespace Perkify.Core.Tests
{
    public class DelegationTests
    {
        const string SkipOrNot = null;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestDelegation(bool eligible)
        {
            var delegation = new Delegation(() => eligible);
            delegation.IsEligible.Should().Be(eligible);
        }

        [Fact]
        public void TestCheckEligible()
        {
            var delgation = new Delegation(() => true) as IEligible;
            var action = new Action(() => (delgation as IEligible).Check());
            action.Should().NotThrow();
        }

        [Fact]
        public void TestCheckIneligible()
        {
            var delgation = new Delegation(() => false) as IEligible;
            var action = new Action(() => (delgation as IEligible).Check());
            action.Should().Throw<InvalidOperationException>().WithMessage("Ineligible state.");
        }
    }
}
