namespace Perkify.Core.Tests
{
    using System.Globalization;

    /*
    public class ExpiryExtensionsTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestGetDeadlineUtc(string expiryUtcString, string gracePeriodString, string deadlineUtcString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var gracePeriod = TimeSpan.Parse(gracePeriodString, CultureInfo.InvariantCulture);
            var expected = DateTime.Parse(deadlineUtcString, CultureInfo.InvariantCulture).ToUniversalTime();

            var mock = new Mock<IExpiry>();
            mock.SetupGet(x => x.ExpiryUtc).Returns(expiryUtc);
            mock.SetupGet(x => x.GracePeriod).Returns(gracePeriod);
            var expiry = mock.Object;

            var actual = expiry.DeadlineUtc;
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "2024-06-09T17:00:00Z", true)]
        public void TestIsExpired(string nowUtcString, string expiryUtcString, string? suspensionUtcString, bool expected)
        {
            var nowUtc = DateTime.Parse(nowUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var suspensionUtc = suspensionUtcString != null ? DateTime.Parse(suspensionUtcString, CultureInfo.InvariantCulture).ToUniversalTime() : (DateTime?)null;

            var mock = new Mock<IExpiry>();
//            mock.SetupGet(x => x.NowUtc).Returns(nowUtc);
            mock.SetupGet(x => x.ExpiryUtc).Returns(expiryUtc);
            mock.SetupGet(x => x.SuspensionUtc.Returns(suspensionUtc);
            var expiry = mock.Object;

            var actual = expiry.IsExpired();
            Assert.Equal(expected, actual);
        }
    }
    */
}