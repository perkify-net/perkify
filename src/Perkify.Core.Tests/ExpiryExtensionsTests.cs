namespace Perkify.Core.Tests
{
    using System.Globalization;

    public class MockExpiry : INowUtc, IExpiry<MockExpiry>
    {
        public DateTime NowUtc { get; set; }

        public DateTime ExpiryUtc { get; set; }

        public TimeSpan GracePeriod { get; set; }

        public TimeSpan Remaining { get; set; }

        public TimeSpan Overdue { get; set; }

        public Renewal? Renewal { get; set; }

        public DateTime? SuspensionUtc { get; set; }

        public bool IsActive { get; set; }

        public MockExpiry Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            throw new NotImplementedException();
        }

        public MockExpiry Deactivate(DateTime? suspensionUtc = null)
        {
            throw new NotImplementedException();
        }

        public MockExpiry Renew(Renewal? renewal)
        {
            throw new NotImplementedException();
        }
    }

    public class ExpiryExtensionsTests
    {
        [Theory]
        [InlineData("2024-06-09T16:00:00Z", "02:00:00", "2024-06-09T18:00:00Z")]
        public void TestGetDeadlineUtc(string expiryUtcString, string gracePeriodString, string deadlineUtcString)
        {
            var expiryUtc = DateTime.Parse(expiryUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var gracePeriod = TimeSpan.Parse(gracePeriodString, CultureInfo.InvariantCulture);
            var expected = DateTime.Parse(deadlineUtcString, CultureInfo.InvariantCulture).ToUniversalTime();
            var expiry = new MockExpiry
            {
                ExpiryUtc = expiryUtc,
                GracePeriod = gracePeriod
            };
            var actual = expiry.GetDeadlineUtc();
            Assert.Equal(expected, actual);
        }

        [Theory]
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
            var expiry = new MockExpiry
            {
                NowUtc = nowUtc,
                ExpiryUtc = expiryUtc,
                SuspensionUtc = suspensionUtc,
            };
            var actual = expiry.IsExpired();
            Assert.Equal(expected, actual);
        }
    }
}