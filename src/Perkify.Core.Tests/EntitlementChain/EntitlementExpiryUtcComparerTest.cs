namespace Perkify.Core.Tests
{
    using NodaTime.Text;

    public class EntitlementExpiryUtcComparerTests
    {
        [Theory]
        [InlineData("2024-10-09T15:00:00Z", +1, -1)]
        [InlineData("2024-10-09T15:00:00Z", -2, +1)]
        [InlineData("2024-10-09T15:00:00Z", 0, 0)]
        public void TestComparer(string nowUtcString, int expiryUtcOffsetInHours, int expected)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();

            var x = new Entitlement(AutoRenewalMode.None, null)
            {
                Expiry = new Expiry(nowUtc),
            };

            var y = new Entitlement(AutoRenewalMode.None, null)
            {
                Expiry = new Expiry(nowUtc.AddHours(expiryUtcOffsetInHours)),
            };

            var comparer = new EntitlementExpiryUtcComparer();
            comparer.Compare(x, y).Should().Be(expected);
        }

        [Theory]
        [InlineData("2024-10-09T15:00:00Z", 1)]
        public void TestComparerNull(string nowUtcString, int expected)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var withExpiryUtc = new Entitlement(AutoRenewalMode.None, null)
            {
                Expiry = new Expiry(nowUtc),
            };
            var withoutExpiryUtc = new Entitlement(AutoRenewalMode.None, null);

            var comparer = new EntitlementExpiryUtcComparer();
            comparer.Compare(withExpiryUtc, null).Should().Be(expected);
            comparer.Compare(null, withExpiryUtc).Should().Be(-expected);
        }

        [Theory]
        [InlineData("2024-10-09T15:00:00Z", 1)]
        public void TestComparerWithoutExpiryUtc(string nowUtcString, int expected)
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var withExpiryUtc = new Entitlement(AutoRenewalMode.None, null)
            {
                Expiry = new Expiry(nowUtc),
            };
            var withoutExpiryUtc = new Entitlement(AutoRenewalMode.None, null);

            var comparer = new EntitlementExpiryUtcComparer();
            comparer.Compare(withExpiryUtc, withoutExpiryUtc).Should().Be(expected);
            comparer.Compare(withoutExpiryUtc, withExpiryUtc).Should().Be(-expected);
        }

        [Fact]
        public void TestComparerNullAndWithoutExpiryUtc()
        {
            var withoutExpiryUtc = new Entitlement(AutoRenewalMode.None, null);
            var comparer = new EntitlementExpiryUtcComparer();
            comparer.Compare(null, null).Should().Be(0);
            comparer.Compare(null, withoutExpiryUtc).Should().Be(0);
            comparer.Compare(withoutExpiryUtc, null).Should().Be(0);
            comparer.Compare(withoutExpiryUtc, withoutExpiryUtc).Should().Be(-0);
        }
    }
}
