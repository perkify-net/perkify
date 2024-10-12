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
    }
}
