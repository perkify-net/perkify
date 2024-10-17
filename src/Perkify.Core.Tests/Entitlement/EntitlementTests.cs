namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

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

        public class TestPrimaryConstructor(int a)
        {
            public int A { get; } = a;
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsEligible
        (
            [CombinatorialValues(true, false)] bool isBalanceEligible,
            [CombinatorialValues(true, false)] bool isExpryEligible,
            [CombinatorialValues(true, false)] bool isEnablementEligible,
            [CombinatorialValues(true, false)] bool iskPrerequesiteEligible
        )
        {
            var mockBalance = new Mock<Balance>(MockBehavior.Strict, 0L, BalanceExceedancePolicy.Reject);
            mockBalance.SetupGet(balance => balance.IsEligible).Returns(isBalanceEligible);
            var mockExpiry = new Mock<Expiry>(MockBehavior.Strict, DateTime.UtcNow, null);
            mockExpiry.SetupGet(expiry => expiry.IsEligible).Returns(isExpryEligible);
            var mockEnablement = new Mock<Enablement>(MockBehavior.Strict, true);
            mockEnablement.SetupGet(enablement => enablement.IsEligible).Returns(isEnablementEligible);
            var mockPrerequesite = new Mock<IEligible>();
            mockPrerequesite.SetupGet(eligible =>eligible.IsEligible).Returns(iskPrerequesiteEligible);
            var entitlement = new Entitlement(AutoRenewalMode.None)
            {
                Balance = mockBalance.Object,
                Expiry = mockExpiry.Object,
                Enablement = mockEnablement.Object,
                Prerequesite = mockPrerequesite.Object,
            };

            var expected = new[] { isBalanceEligible, isExpryEligible, isEnablementEligible, iskPrerequesiteEligible }.All(eligible => eligible);
            entitlement.IsEligible.Should().Be(expected);
        }

        [Fact(Skip = SkipOrNot)]
        public void TestIsEligibleNullReference()
        {
            var entitlement = new Entitlement(AutoRenewalMode.None);
            entitlement.IsEligible.Should().BeTrue();
        }
    }
}
