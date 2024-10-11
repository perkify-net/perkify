namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class ExpiryTests
    {
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
    }
}
