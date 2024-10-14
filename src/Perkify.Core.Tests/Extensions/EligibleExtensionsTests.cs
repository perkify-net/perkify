namespace Perkify.Core.Tests
{
    /*
    public class EligibleExtensionsTests
    {
        const string SkipOrNot = null;

        [Fact(Skip = SkipOrNot)]
        public void TestCheckEligible()
        {
            var mock = new Mock<IEligible>();
            mock.Setup(x => x.IsEligible).Returns(true);
            var mockEligibleObject = mock.Object;
            var action = new Action(() => mockEligibleObject.Check());
            action.Should().NotThrow();
        }

        [Fact(Skip = SkipOrNot)]
        public void TestCheckIneligible()
        {
            var mock = new Mock<IEligible>();
            mock.Setup(x => x.IsEligible).Returns(false);
            var mockEligibleObject = mock.Object;
            var action = new Action(() => mockEligibleObject.Check());
            action.Should().Throw<InvalidOperationException>().WithMessage("Ineligible state.");
        }
    }
    */
}
