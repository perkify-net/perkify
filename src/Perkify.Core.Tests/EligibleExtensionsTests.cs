namespace Perkify.Core.Tests
{
    public class EligibleExtensionsTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(true)]
        public void TestCheck(bool eligible)
        {
            var delegation = new Delegation(() => eligible);
            var action = new Action(() => delegation.Check());
            action.Should().NotThrow();
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData(false)]
        public void TestCheckIneligible(bool eligible)
        {
            var delegation = new Delegation(() => eligible);
            var action = new Action(() => delegation.Check());
            action.Should().Throw<InvalidOperationException>().WithMessage("");
        }
    }
}
