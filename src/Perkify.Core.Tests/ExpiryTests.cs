namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public class ExpiryTests
    {
        #region Create Expiry w/o Suspension

        [Theory]
        [InlineData(null, "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData(null, "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestCreateExpiry(string? nowUtc, string expiryUtcString, string? graceString, string deadlineUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var deadlineUtc = InstantPattern.General.Parse(deadlineUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);
            Assert.Equal(grace ?? TimeSpan.Zero, expiry.GracePeriod);
            Assert.Equal(deadlineUtc, expiry.GetDeadlineUtc());
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", null, "2024-06-09T15:00:00Z", "2024-06-09T15:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "2024-06-09T17:00:00Z")]
        public void TestCreateExpiryWithNowUtc(string nowUtcString, string? graceString, string expiryUtcString, string deadlineUtcString)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value;
            var clock = new FakeClock(nowUtc);
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var deadlineUtc = InstantPattern.General.Parse(deadlineUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(null, grace, clock);
            Assert.Equal(nowUtc.ToDateTimeUtc(), expiry.ExpiryUtc);
            Assert.Equal(grace ?? TimeSpan.Zero, expiry.GracePeriod);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestCreateExpiryWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(suspensionUtc, expiry.SuspensionUtc);
            Assert.True(expiry.SuspensionUtc <= expiry.GetDeadlineUtc());
            Assert.False(expiry.IsActive);
        }

        [Theory]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z")]
        [InlineData("2024-06-09T21:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T20:00:00Z")]
        public void TestCreateExpiryWithSuspensionUtcAfterDeadline(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.NotEqual(suspensionUtc, expiry.SuspensionUtc);
            Assert.Equal(expiry.GetDeadlineUtc(), expiry.SuspensionUtc);
            Assert.False(expiry.IsActive);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestCreateExpiryWithSuspensionUtcResuspending(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.NotNull(expiry.SuspensionUtc);
            Assert.Throws<InvalidOperationException>(() => expiry.WithSuspensionUtc(suspensionUtc));
        }

        [Theory]
        [InlineData("2024-06-09T14:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z")]
        [InlineData("2024-06-09T14:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z")]
        public void TestCreateExpiryWithSuspensionUtcInvalidFutureTime(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Null(expiry.SuspensionUtc);

            Assert.Throws<ArgumentOutOfRangeException>(() => expiry.WithSuspensionUtc(suspensionUtc));
            Assert.Null(expiry.SuspensionUtc);
        }

        [Theory]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestExpirySuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.False(expiry.IsEligible);
            Assert.True(expiry.IsActive);
            Assert.Equal(suspensionUtc, expiry.SuspensionUtc);
        }

        #endregion

        #region IEligible Tests

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", false)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", false)]
        public void TestExpiryElligible(string nowUtc, string expiryUtcString, string? graceString, bool eligible)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(eligible, expiry.IsEligible);
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", false)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", false)]
        public void TestExpiryElligibleWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, bool eligible)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(eligible, expiry.IsEligible);
        }

        #endregion

        #region Expired, Overdue & Remaining

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", true)]
        public void TestExpiryExpired(string nowUtcString, string expiryUtcString, string? graceString, bool expired)
        {
            var clock = nowUtcString != null ? new FakeClock(InstantPattern.General.Parse(nowUtcString).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);
            Assert.Equal(expired, expiry.IsExpired());
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T14:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", false)]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", true)]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", true)]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", true)]
        public void TestExpiryExpiredWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, bool expired)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expired, expiry.IsExpired());
        }


        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "01:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "02:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "02:00:00")]
        public void TestExpiryOverdue(string nowUtc, string expiryUtcString, string? graceString, string overdueString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var overdue = TimeSpan.Parse(overdueString, CultureInfo.InvariantCulture);

            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(overdue, expiry.Overdue);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T17:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T16:00:00Z", "00:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T17:00:00Z", "01:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z", "02:00:00")]
        [InlineData("2024-06-09T20:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T19:00:00Z", "02:00:00")]
        public void TestExpiryOverdueWithSuspensionUtc(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString, string overdueString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var overdue = TimeSpan.Parse(overdueString, CultureInfo.InvariantCulture);

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.Equal(overdue, expiry.Overdue);
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "01:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "00:00:00")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "01:00:00")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "00:00:00")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-01:00:00")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-02:00:00")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "-02:00:00")]
        public void TestExpiryRemaining(string nowUtc, string expiryUtcString, string? graceString, string remainingString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var remaining = TimeSpan.Parse(remainingString, CultureInfo.InvariantCulture);
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.Equal(remaining, expiry.Remaining);
        }

        [Theory]
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

        #endregion

        #region Renew

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00","PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT1H", false, "2024-06-09T17:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", null, "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T17:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T18:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        [InlineData("2024-06-09T19:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "P1M", true, "2024-07-09T16:00:00Z")]
        public void TestExpiryRenew(string nowUtc, string expiryUtcString, string? graceString, string duration, bool calendar, string expectedString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var expected = InstantPattern.General.Parse(expectedString).Value.ToDateTimeUtc();

            var expiry = new Expiry(expiryUtc, grace, clock);
            var renewal = new Renewal(duration, calendar);
            expiry.Renew(renewal);
            Assert.Equal(expected, expiry.ExpiryUtc);
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "PT-1H", false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "PT-1H", false)]
        public void TestExpiryRenewNegativeTimeSpan(string nowUtc, string expiryUtcString, string? graceString, string duration, bool calendar)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;

            var expiry = new Expiry(expiryUtc, grace, clock);
            var renewal = new Renewal(duration, calendar);
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }

        [Theory]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T15:00:00Z", "PT1H", false)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H", true)]
        [InlineData("2024-06-09T15:00:00Z", "2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T15:00:00Z", "PT1H", false)]
        public void TestExpiryRenewSuspended(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcStribng, string duration, bool calendar)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcStribng).Value.ToDateTimeUtc();

            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            var renewal = new Renewal(duration, calendar);
            Assert.False(expiry.IsActive);
            Assert.Throws<InvalidOperationException>(() => expiry.Renew(renewal));
        }

        #endregion

        #region Suspend & Resume

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-11T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", "2024-06-11T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-13T16:00:00Z", "2024-06-11T16:00:00Z")]
        public void TestDeactivateExpiry(string expiryUtcString, string? graceString, string suspensionUtcString, string expectedUtcString)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expectedUtc = InstantPattern.General.Parse(expectedUtcString).Value.ToDateTimeUtc();

            var nowUtc = suspensionUtc.AddHours(1);
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);

            expiry.Deactivate(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expectedUtc, expiry.SuspensionUtc);
            Assert.False(expiry.IsEligible);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-11T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", "2024-06-11T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-13T16:00:00Z", "2024-06-11T16:00:00Z")]
        public void TestDeactivateExpiryAfterDeadline(string expiryUtcString, string? graceString, string suspensionUtcString, string expectedUtcString)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expectedUtc = InstantPattern.General.Parse(expectedUtcString).Value.ToDateTimeUtc();

            var deadlineUtc = suspensionUtc + (grace ?? TimeSpan.Zero);
            var nowUtc = deadlineUtc.AddHours(1);
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);

            expiry.Deactivate(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expectedUtc, expiry.SuspensionUtc);
            Assert.False(expiry.IsEligible);
        }

        [Theory, CombinatorialData]
        public void TestDeactivateExpiryImplicitSuspensionUtc
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues("3:00:00", "2:00:00", "1:00:00", "00:00:00", "-1:00:00")] string? nowUtcIfHaving
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var gracePeriod = gracePeriodIfHaving is null ? (TimeSpan?)null : TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture);
            var nowUtc = nowUtcIfHaving is null ? (DateTime?)null : expiryUtc.Add(TimeSpan.Parse(nowUtcIfHaving, CultureInfo.InvariantCulture));
            var clock = nowUtc is null ? null : new FakeClock(nowUtc.Value.ToInstant());
            var expiry = new Expiry(expiryUtc, gracePeriod, clock);
            Assert.True(expiry.IsActive);

            expiry.Deactivate(null);
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.SuspensionUtc);
            Assert.False(expiry.IsEligible);
        }

        [Theory]
        [InlineData("2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-11T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-11T16:00:00Z", "2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z")]
        [InlineData("2024-06-13T16:00:00Z", "2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z")]
        public void TestDeactivateExpiryExplicitNow(string nowUtcString, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);

            expiry.Deactivate(nowUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(suspensionUtc, expiry.SuspensionUtc);
            Assert.False(expiry.IsEligible);
        }

        [Theory]
        [InlineData("2024-06-08T16:00:00Z", "2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z")]
        [InlineData("2024-06-08T16:00:00Z", "2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z")]
        public void TestDeactivateExpiryAlreadySuspended(string nowUtc, string expiryUtcString, string? graceString, string suspensionUtcString)
        {
            var clock = nowUtc != null ? new FakeClock(InstantPattern.General.Parse(nowUtc).Value) : null;
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            expiry.Deactivate(suspensionUtc.AddDays(1));
            Assert.Equal(suspensionUtc, expiry.SuspensionUtc);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", "2024-06-11T16:00:00Z")]
        public void TestResumeExpiry(string expiryUtcString, string? graceString, string suspensionUtcString, string resumptionUtcString)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var resumptionUtc = InstantPattern.General.Parse(resumptionUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(resumptionUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);

            expiry.Activate(resumptionUtc, extended:false);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);
            Assert.True(expiry.IsActive);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z", "2024-06-11T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z", "2024-06-12T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z", "2024-06-11T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z", "2024-06-12T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", "2024-06-11T16:00:00Z", "2024-06-09T16:00:00Z")]
        public void TestResumeExpiryExtended(string expiryUtcString, string? graceString, string suspensionUtcString, string resumptionUtcString, string expectedUtcString)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var resumptionUtc = InstantPattern.General.Parse(resumptionUtcString).Value.ToDateTimeUtc();
            var expectedUtc = InstantPattern.General.Parse(expectedUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(resumptionUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);

            expiry.Activate(resumptionUtc, extended: true);
            Assert.Equal(expectedUtc, expiry.ExpiryUtc);
            Assert.True(expiry.IsActive);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-08T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-09T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", "2024-06-10T16:00:00Z")]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", "2024-06-11T16:00:00Z")]
        public void TestResumeExpiryImplicitNow(string expiryUtcString, string? graceString, string suspensionUtcString, string resumptionUtcString)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var resumptionUtc = InstantPattern.General.Parse(resumptionUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(resumptionUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);

            expiry.Activate(null, extended: false);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);
            Assert.True(expiry.IsActive);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", false)]
        public void TestResumeExpiryInvalidResumptionUtc(string expiryUtcString, string? graceString, string suspensionUtcString, bool extended)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var suspensionUtc = InstantPattern.General.Parse(suspensionUtcString).Value.ToDateTimeUtc();
            var resumptionUtc = suspensionUtc.AddHours(-1);
            var clock = new FakeClock(suspensionUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock).WithSuspensionUtc(suspensionUtc);
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);

            var elapsed = resumptionUtc - suspensionUtc;
            clock.Advance(elapsed.ToDuration());
            Assert.Throws<ArgumentOutOfRangeException>(() => expiry.Activate(resumptionUtc, extended: extended));
            Assert.False(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);
        }

        [Theory]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-07T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", null, "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", true)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-07T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-09T16:00:00Z", false)]
        [InlineData("2024-06-09T16:00:00Z", "2:00:00:00", "2024-06-11T16:00:00Z", false)]
        public void TestResumeExpiryUnsuspended(string expiryUtcString, string? graceString, string resumptionUtcString, bool extended)
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = graceString != null ? TimeSpan.Parse(graceString, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var resumptionUtc = InstantPattern.General.Parse(resumptionUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(resumptionUtc.ToInstant());
            var expiry = new Expiry(expiryUtc, grace, clock);
            Assert.True(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);

            expiry.Activate(resumptionUtc, extended);
            Assert.True(expiry.IsActive);
            Assert.Equal(expiryUtc, expiry.ExpiryUtc);
        }

        #endregion
    }
}
