namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

public partial class ExpiryTests
{
    [Theory, CombinatorialData]
    public void TestIsExpired
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
        [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffset
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : TimeSpan.Zero;
        var nowUtc = expiryUtc.AddHours(nowUtcOffset);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        var expected = nowUtcOffset >= 0;
        expiry.IsExpired.Should().Be(expected);
    }

    [Theory, CombinatorialData]
    public void TestRemainingNoGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(-1, 0, +1)] int nowUtcOffset
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var nowUtc = expiryUtc.AddHours(nowUtcOffset);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, null) { Clock = clock };
        var expected = nowUtcOffset < 0 ? TimeSpan.FromHours(-nowUtcOffset) : TimeSpan.Zero;
        expiry.Remaining(false).Should().Be(expected);
        expiry.Remaining(true).Should().Be(expected);
    }

    [Theory, CombinatorialData]
    public void TestRemainingWithinGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(+2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1, +2)] int nowUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var deadlineUtc = expiryUtc + grace;
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());

        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        var expectRemainingUntilExpiry = TimeSpan.FromHours(Math.Max(-nowUtcOffsetInHours, 0));
        var expectRemainingUntilDeadline = TimeSpan.FromHours(Math.Max(-nowUtcOffsetInHours + gracePeriodInHours, 0));
        expiry.Remaining(false).Should().Be(expectRemainingUntilExpiry);
        expiry.Remaining(true).Should().Be(expectRemainingUntilDeadline);
    }

    [Theory, CombinatorialData]
    public void TestOverdueNoGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(-1, 0, +1)] int nowUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiry = new Expiry(expiryUtc, null) { Clock = clock };
        expiry.Overdue.Should().Be(TimeSpan.Zero);
    }

    [Theory, CombinatorialData]
    public void TestOverdueWithinGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(+2)] int gracePeriodInHours,
        [CombinatorialValues(-1, 0, +1)] int nowUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());
        var overdue = nowUtcOffsetInHours > 0 ? TimeSpan.FromHours(nowUtcOffsetInHours) : TimeSpan.Zero;
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        expiry.Overdue.Should().Be(overdue);
    }

    [Theory, CombinatorialData]
    public void TestOverdueAfterGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(+2)] int gracePeriodInHours,
        [CombinatorialValues(+2, +3)] int nowUtcOffsetInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var grace = TimeSpan.FromHours(gracePeriodInHours);
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = grace };
        var overdue = nowUtcOffsetInHours > 0 ? TimeSpan.FromHours(gracePeriodInHours) : TimeSpan.Zero;
        expiry.Overdue.Should().Be(overdue);
    }

    [Theory, CombinatorialData]
    public void TestSetGracePeriod
    (
        [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
        [CombinatorialValues(null, +2, +5)] int? initialGracePeriodInHours,
        [CombinatorialValues(-1)] int nowUtcOffsetInHours,
        [CombinatorialValues(null, +2, +5)] int? expectedGracePeriodInHours
    )
    {
        var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
        var initial = initialGracePeriodInHours != null ? TimeSpan.FromHours(initialGracePeriodInHours.Value) : TimeSpan.Zero;
        var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
        var clock = new FakeClock(nowUtc.ToInstant());
        var expiry = new Expiry(expiryUtc, clock) { GracePeriod = initial };
        expiry.GracePeriod.Should().Be(initial);

        var expected = expectedGracePeriodInHours != null ? TimeSpan.FromHours(expectedGracePeriodInHours.Value) : TimeSpan.Zero;
        expiry.GracePeriod = expected;
        expiry.GracePeriod.Should().Be(expected);
    }
}
