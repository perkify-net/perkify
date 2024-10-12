namespace Perkify.Core.Tests
{
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
    using System.Globalization;

    public class RenewalTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData("P1M", true)]
        [InlineData("PT1H", false)]
        public void TestCreateRenewal(string duration, bool calendar)
        {
            var renewal = new Renewal(duration, calendar);
            renewal.Duration.Should().Be(duration);
            renewal.Calendar.Should().Be(calendar);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("INCORRECT", true)]
        [InlineData("INCORRECT", false)]
        public void TestCreateRenewalInvalidFormat(string duration, bool calendar)
        {
            var action = () => new Renewal(duration, calendar);
            action
                .Should()
            .Throw<FormatException>()
            .WithMessage("Incorrect ISO8601 duration string.");
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("P1M", true, "2024-06-09T17:00:00Z", "2024-07-09T17:00:00Z")]
        [InlineData("PT1H", false, "2024-06-09T17:00:00Z", "2024-06-09T18:00:00Z")]
        public void TestRenewRenewal(string duration, bool calendar, string expiryUtcString, string expectedUtcString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expected = DateTime.Parse(expectedUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var renewal = new Renewal(duration, calendar);
            var actual = renewal.Renew(expiryUtc);
            actual.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("P1M", true, "2024-06-09T17:00:00Z", "31.00:00:00")]
        [InlineData("PT1H", false, "2024-06-09T17:00:00Z", "01:00:00")]
        public void TestTillRenewal(string duration, bool calendar, string expiryUtcString, string expectedString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expected = TimeSpan.Parse(expectedString, CultureInfo.InvariantCulture);
            var renewal = new Renewal(duration, calendar);
            var actual = renewal.Till(expiryUtc);
            actual.Should().Be(expected);
        }
    }
}
