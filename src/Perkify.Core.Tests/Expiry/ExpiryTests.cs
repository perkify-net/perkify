namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
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
    }
}
