namespace Perkify.Core.Tests
{
    using System.Globalization;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public class EntitlementTests
    {
        const string SkipOrNot = "Skipped";

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEntitlement
        (
            [CombinatorialValues(0, 100)] long balanceAmount,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string expiryUtcString,
            [CombinatorialValues(null, "02:00:00")] string? gracePeriodIfHaving,
            [CombinatorialValues(3, 0, -3)] long nowUtcOffset,
            [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.Default, AutoRenewalMode.All)] AutoRenewalMode autorenewal,
            [CombinatorialValues(null, true, false)] bool? prerequisiteIfHaving
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var gracePeriod = gracePeriodIfHaving is null ? (TimeSpan?)null : TimeSpan.Parse(gracePeriodIfHaving, CultureInfo.InvariantCulture);
            var nowUtc = expiryUtc.AddHours(nowUtcOffset);
            var clock = new FakeClock(nowUtc.ToInstant());

            var balance = Balance.Debit().WithBalance(incoming:balanceAmount, outgoing:0L);
            var expiry = new Expiry(expiryUtc, gracePeriod) { Clock = clock! };
            var activator = new Enablement();
            var prerequisite = prerequisiteIfHaving is null ? null : new Delegation(() => prerequisiteIfHaving.Value);
            var entitlement = new Entitlement(autorenewal)
            {
                Balance = balance,
                Expiry = expiry,
                Enablement = activator,
                Prerequesite = prerequisite
            };

            Assert.Equal(balanceAmount, entitlement.Gross);
            Assert.Equal(expiryUtc, entitlement.ExpiryUtc);
            Assert.Equal(gracePeriod ?? TimeSpan.Zero, entitlement.GracePeriod);
            Assert.Equal(autorenewal, entitlement.AutoRenewalMode);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEntitlementWithoutExpiry
        (
            [CombinatorialValues(0, 100)] long balanceAmount,
            [CombinatorialValues(null, true, false)] bool? prerequisiteIfHaving
        )
        {
            var balance = Balance.Debit().WithBalance(incoming: balanceAmount, outgoing: 0L);
            var prerequisite = prerequisiteIfHaving is null ? null : new Delegation(() => prerequisiteIfHaving.Value);
            var entitlement = new Entitlement()
            {
                Balance = balance,
                Prerequesite = prerequisite,
            };
            Assert.Equal(balanceAmount, entitlement.Gross);
            Assert.Equal(DateTime.MaxValue, entitlement.ExpiryUtc);
            Assert.Equal(TimeSpan.Zero, entitlement.GracePeriod);
            Assert.Equal(AutoRenewalMode.None, entitlement.AutoRenewalMode);
        }
    }
}
