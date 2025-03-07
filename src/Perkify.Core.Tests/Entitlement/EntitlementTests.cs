namespace Perkify.Core.Tests;

using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

public partial class EntitlementTests
{
    [Theory, CombinatorialData]
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
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = Balance.Debit().WithBalance(gross, 0L),
            Expiry = new Expiry(expiryUtc),
            Enablement = new Enablement(isActive),
            Prerequesite = new Delegation(() => prerequisite),
        };

        entitlement.AutoRenewalMode.Should().Be(renewal);
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);

        entitlement.HasBalance.Should().BeTrue();
        entitlement.HasExpiry.Should().BeTrue();
        entitlement.HasEnablement.Should().BeTrue();
        entitlement.Prerequesite.Should().NotBeNull();

        entitlement.Gross.Should().Be(gross);
        entitlement.ExpiryUtc.Should().Be(expiryUtc);
        entitlement.IsActive.Should().Be(isActive);
        entitlement.Prerequesite!.IsEligible.Should().Be(prerequisite);
    }

    [Theory, CombinatorialData]
    public void TestCreateEntitlementNotInitialized
    (
        [CombinatorialValues(AutoRenewalMode.None)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var entitlement = new Entitlement(renewal, clock);
        entitlement.AutoRenewalMode.Should().Be(renewal);
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);

        entitlement.HasBalance.Should().BeFalse();
        entitlement.HasExpiry.Should().BeFalse();
        entitlement.HasEnablement.Should().BeFalse();
        entitlement.Prerequesite.Should().BeNull();

        var operations = new Action[]
        {
                // OPs: IBalance
                () => _ = entitlement.Threshold,
                () => _ = entitlement.BalanceExceedancePolicy,
                () => _ = entitlement.BalanceType,
                () => _ = entitlement.Incoming,
                () => _ = entitlement.Outgoing,
                () => _ = entitlement.Gross,
                () => _ = entitlement.Overspending,
                // OPs: IExpiry
                () => _ = entitlement.ExpiryUtc,
                () => _ = entitlement.GracePeriod,
                () => entitlement.GracePeriod = TimeSpan.Zero,
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

    [Theory, CombinatorialData]
    public void TestCreateEntitlementBalanceNotInitialized
    (
        [CombinatorialValues(AutoRenewalMode.None)] AutoRenewalMode renewal,
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var entitlement = new Entitlement(renewal, clock);
        entitlement.AutoRenewalMode.Should().Be(renewal);
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);

        entitlement.HasBalance.Should().BeFalse();
        entitlement.HasExpiry.Should().BeFalse();
        entitlement.HasEnablement.Should().BeFalse();
        entitlement.Prerequesite.Should().BeNull();

        var operations = new Action[]
        {
                // OPs: IBalance
                () => entitlement.Topup(0L),
                () => _ = entitlement.Deduct(0L),
                () => entitlement.Adjust(null, null),
                () => entitlement.Clear(),
        };
        foreach (var operation in operations)
        {
            operation.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Balance is not initialized.");
        }
    }

    [Theory, CombinatorialData]
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
        var entitlement = new Entitlement(renewal, clock)
        {
            Balance = Balance.Debit().WithBalance(gross, 0L),
            Expiry = new Expiry(expiryUtc),
            Enablement = new Enablement(isActive),
            Prerequesite = new Delegation(() => prerequisite),
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

    [Theory, CombinatorialData]
    public void TestCreateEntitlementWithFakedClock
    (
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var entitlement = new Entitlement(AutoRenewalMode.None, null)
        {
            Expiry = new Expiry(DateTime.UtcNow),
            Enablement = new Enablement(true),
        }.WithClock(clock);

        // TODO: Test inner clocks within Expiry & Enablement.
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
    }

    [Theory, CombinatorialData]
    public void TestCreateEntitlementWithSystemClock
    (
        [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
    )
    {
        var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
        var clock = new FakeClock(nowUtc.ToInstant());
        var entitlement = new Entitlement(AutoRenewalMode.None, clock)
        {
            Expiry = new Expiry(DateTime.UtcNow),
            Enablement = new Enablement(true),
        }.WithClock(null);

        // TODO: Test inner clocks within Expiry & Enablement.
        entitlement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
