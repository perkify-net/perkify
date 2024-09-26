namespace Perkify.Core.UnitTests
{
    public class RenewalTests
    {
        [Theory]
        [InlineData("INCORRECT", true)]
        [InlineData("INCORRECT", false)]
        public void TestCreateRenewal(string duration, bool calendar)
        {
            Assert.Throws<FormatException>(() => new Renewal(duration, calendar));
        }
    }
}
