namespace Perkify.Core.Tests
{
    /*
            [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H!")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H!")]
        public void TestExpiryRenewSuspended(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcStribng, string duration)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcStribng).Value.ToDateTimeUtc();

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock! }.WithSuspensionUtc(suspensionUtc);
            var renewal = new Renewal(duration, calendar);
            Assert.False(expiry.IsActive);
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }
 
    
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsExpiredWithSuspensionUtc
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-3, -2, -1, 0, +1)] int nowUtcOffset,
            [CombinatorialValues(-4, -3, -2, -1, 0)] int suspensionUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());
            var suspensionUtc = expiryUtc.AddHours(suspensionUtcOffset);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            var expected = suspensionUtcOffset >= 0;
            expiry.IsExpired().Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", "02:00:00")]
        public void TestOverdueWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string overdueString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var overdue = TimeSpan.Parse(overdueString, CultureInfo.InvariantCulture);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(overdue, expiry.Overdue);
        }

        [Theory(Skip = SkipOrNot)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T14:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T14:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", "-01:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", "-02:00:00")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", "-02:00:00")]
        public void TestExpiryRemainingWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string remainingString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var remaining = TimeSpan.Parse(remainingString, CultureInfo.InvariantCulture);
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(remaining, expiry.Remaining);
        }

    [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateExpiryWithSuspensionUtc
        (
            [CombinatorialValues("2024-06-09T16:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-1, 0, +1, +2, +3)] int nowUtcOffset,
            [CombinatorialValues(-2, -1, 0, +1, +2, +3, +4)] int suspensionUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());
            var suspensionUtc = expiryUtc.AddHours(suspensionUtcOffset);

            var expiry = new Expiry(expiryUtc, grace) { Clock = clock }.WithSuspensionUtc(suspensionUtc);
            expiry.DeactivationUtc.HasValue.Should().BeTrue();
            var deadlineUtc = expiry.GetDeadlineUtc();
            var expected = suspensionUtc < deadlineUtc ? suspensionUtc : deadlineUtc;
            expiry.DeactivationUtc!.Value.Should().Be(expected);
            expiry.DeactivationUtc!.Value.Should().BeOnOrBefore(deadlineUtc);
            expiry.IsActive.Should().BeFalse();
        }
        */

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
            /*
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
        */
}