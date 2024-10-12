namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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
    }
}
