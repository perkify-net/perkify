namespace Perkify.Core.Tests
{
    public class ChronoIntervalTests
    {
        const string SkipOrNot = null;

        [Theory]
        [InlineData("P1M", true)]
        [InlineData("PT1H!", false)]
        public void TestCreateInterval(string duration, bool calendar)
        {
            var renewal = new ChronoInterval(duration);
            renewal.Duration.Should().Be(duration);
            renewal.Calendar.Should().Be(calendar);
        }

        [Theory, CombinatorialData]
        public void TestCreateIntervalTrimmed
        (
            [CombinatorialValues("P1M", "PT1H!")] string duration,
            [CombinatorialValues(0, 1)] int prefix,
            [CombinatorialValues(0, 1)] int suffix
        )
        {
            var input = $"{new string(' ', prefix)}{duration}{new string(' ', suffix)}";
            var renewal = new ChronoInterval(input);
            renewal.Duration.Should().Be(duration);
        }

        [Theory]
        [InlineData("INCORRECT")]
        [InlineData("INCORRECT!")]
        [InlineData("!")]
        [InlineData(" ! ")]
        public void TestCreateIntervalIso8601IsIncorrect(string duration)
        {
            var action = () => new ChronoInterval(duration);
            action
                .Should()
            .Throw<FormatException>()
            .WithMessage("Incorrect ISO8601 duration string.");
        }

        [Fact]
        public void TestCreateIntervalIso8601IsNull()
        {
            var iso8601 = (string)null!;
            var parameter = nameof(iso8601);

            var action = () => new ChronoInterval(iso8601);
            action
                .Should()
                .Throw<ArgumentNullException>()
            .WithMessage($"Value cannot be null. (Parameter '{parameter}')");
        }

        [Theory]
        [InlineData("P1M", "2024-06-09T17:00:00Z", "2024-07-09T17:00:00Z")]
        [InlineData("PT1H!", "2024-06-09T17:00:00Z", "2024-06-09T18:00:00Z")]
        public void TestRenew(string duration, string expiryUtcString, string expectedUtcString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expected = DateTime.Parse(expectedUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var renewal = new ChronoInterval(duration);
            var actual = renewal.Renew(expiryUtc);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("P1M", "2024-06-09T17:00:00Z", "31.00:00:00")]
        [InlineData("PT1H!", "2024-06-09T17:00:00Z", "01:00:00")]
        public void TestTill(string duration, string expiryUtcString, string expectedString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expected = TimeSpan.Parse(expectedString, CultureInfo.InvariantCulture);
            var renewal = new ChronoInterval(duration);
            var actual = renewal.Till(expiryUtc);
            actual.Should().Be(expected);
        }
    }
}
