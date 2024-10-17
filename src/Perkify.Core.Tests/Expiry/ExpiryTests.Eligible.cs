namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsElligible
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-1, 0, +1)] int nowUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var deadlineUtc = expiryUtc + (grace ?? TimeSpan.Zero);
            var nowUtc = deadlineUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock };
            var expected = nowUtcOffset < 0;
            expiry.IsEligible.Should().Be(expected);
        }
    }
}
