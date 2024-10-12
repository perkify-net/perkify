namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeactivateExpiryWithImplicitSuspensionUtc
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(-3, -2, -1, 0, 1)] int nowUtcOffset
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var grace = gracePeriodIfHaving != null ? TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture) : (TimeSpan?)null;
            var deadlineUtc = expiryUtc + (grace ?? TimeSpan.Zero);
            var nowUtc = deadlineUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());

            var expiry = new Expiry(expiryUtc, grace, clock);
            expiry.IsActive.Should().BeTrue();
            expiry.Deactivate(null);
            expiry.IsActive.Should().BeFalse();

            Assert.Equal(expiryUtc, expiry.SuspensionUtc);
        }

        [Theory(Skip = SkipOrNot)]
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

        [Theory(Skip = SkipOrNot)]
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
    }
}
