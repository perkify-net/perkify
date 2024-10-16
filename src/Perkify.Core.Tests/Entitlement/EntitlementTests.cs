namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEntitlement
        (
            [CombinatorialValues(AutoRenewalMode.None, AutoRenewalMode.Default, AutoRenewalMode.All)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(100L)] long gross,
            [CombinatorialValues(+1)] int expiryUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues(true, false)] bool prerequisite
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
            var entitlement = new Entitlement(renewal)
            {
                Balance = Balance.Debit().WithBalance(gross, 0L),
                Expiry = new Expiry(expiryUtc),
                Enablement = new Enablement(isActive),
                Prerequesite = new Delegation(() => prerequisite),
                Clock = clock,
            };

            entitlement.AutoRenewalMode.Should().Be(renewal);
            entitlement.NowUtc.Should().Be(nowUtc);
            entitlement.Gross.Should().Be(gross);
            entitlement.ExpiryUtc.Should().Be(expiryUtc);
            entitlement.IsActive.Should().Be(isActive);
            entitlement.Prerequesite!.IsEligible.Should().Be(prerequisite);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEntitlementNotInitialized
        (
            [CombinatorialValues(AutoRenewalMode.None)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var entitlement = new Entitlement(renewal) { Clock = clock };
            entitlement.AutoRenewalMode.Should().Be(renewal);
            entitlement.NowUtc.Should().Be(nowUtc);

            var operations = new Action[]
            {
                // OPs: IBalance
                () => _ = entitlement.Incoming,
                () => _ = entitlement.Outgoing,
                () => _ = entitlement.Threshold,
                () => _ = entitlement.BalanceType,
                () => _ = entitlement.Overspending,
                () => entitlement.Topup(0L),
                () => _ = entitlement.Deduct(0L),
                () => entitlement.Adjust(null, null),
                () => entitlement.Clear(),
                // OPs: IExpiry
                () => _ = entitlement.ExpiryUtc,
                () => _ = entitlement.GracePeriod,
                () => entitlement.GracePeriod = null,
                () => _ = entitlement.DeadlineUtc,
                () => _ = entitlement.IsExpired,
                () => _ = entitlement.Overdue,
                () => _ = entitlement.Renewal,
                () => _ = entitlement.Remaining(),
                () => entitlement.Renew(null),
                // OPs: IEnablement
                () => _ = entitlement.IsActive,
                () => _ = entitlement.EffectiveUtc,
                () => _ = entitlement.IsImmediateEffective,
                () => entitlement.Deactivate(),
                () => entitlement.Activate(),
            };

            foreach (var operation in operations)
            {
                operation.Should()
                    .Throw<NullReferenceException>()
                    .WithMessage("Object reference not set to an instance of an object.");
            }
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEntitlementGettersForbidden
        (
            [CombinatorialValues(AutoRenewalMode.None)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(100L)] long gross,
            [CombinatorialValues(+1)] int expiryUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues(true, false)] bool prerequisite
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var expiryUtc = nowUtc.AddHours(expiryUtcOffsetInHours);
            var entitlement = new Entitlement(renewal)
            {
                Balance = Balance.Debit().WithBalance(gross, 0L),
                Expiry = new Expiry(expiryUtc),
                Enablement = new Enablement(isActive),
                Prerequesite = new Delegation(() => prerequisite),
                Clock = clock,
            };

            var balance = () => entitlement.Balance;
            balance.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Access denied (inner balance).");
            var expiry = () => entitlement.Expiry;
            expiry.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Access denied (inner expiry).");
            var enablement = () => entitlement.Enablement;
            enablement.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Access denied (inner enablement).");
        }
    }
}
