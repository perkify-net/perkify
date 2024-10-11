namespace Perkify.Core.Tests
{
    public class EligibleTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestEligible(bool eligible)
        {
            var e = new Delegation(() => eligible);
            Assert.Equal(eligible, e.IsEligible);
        }

        [Theory]
        [InlineData(true)]
        public void TestCheck(bool eligible)
        {
            var e = new Delegation(() => eligible);
            (e as IEligible).Check();
        }

        [Theory]
        [InlineData(false)]
        public void TestCheckIneligible(bool eligible)
        {
            var e = new Delegation(() => eligible);
            Assert.Throws<InvalidOperationException>(() => (e as IEligible).Check());
        }
    }
}
