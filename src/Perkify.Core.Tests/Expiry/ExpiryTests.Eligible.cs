namespace Perkify.Core.Tests
{
    using System.Globalization;
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

            var expiry = new Expiry(expiryUtc, grace, clock);
            var expected = nowUtcOffset < 0;
            expiry.IsEligible.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsElligibleWithSuspensionUtc
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-3, -2, -1, 0, +1)] int nowUtcOffset,
            [CombinatorialValues(-4, -3, -2, -1, 0)] int suspensionUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var deadlineUtc = expiryUtc + (grace ?? TimeSpan.Zero);
            var nowUtc = deadlineUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());
            var suspensionUtc = deadlineUtc.AddHours(suspensionUtcOffset);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            var expected = (suspensionUtcOffset, nowUtcOffset) switch
            {
                // Already suspended
                (int so, int no) when so <= no => false,

                // Will suspend in future
                (int so, int no) when so > no => nowUtcOffset < 0,

                _ => throw new NotImplementedException(),
            };
            expiry.IsEligible.Should().Be(expected);
        }
    }
}
