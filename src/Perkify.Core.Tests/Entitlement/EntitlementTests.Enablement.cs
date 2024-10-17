namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestEnablementProperties
        (
            [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues(+1)] int EffectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var effectiveUtc = nowUtc.AddHours(EffectiveUtcOffsetInHours);
            var enablement = new Enablement(isActive).WithEffectiveUtc(effectiveUtc, isImmediateEffective);
            var entitlement = new Entitlement(renewal)
            {
                Enablement = enablement,
                Clock = clock,
            };

            entitlement.AutoRenewalMode.Should().Be(renewal);
            entitlement.NowUtc.Should().Be(nowUtc);
            entitlement.IsActive.Should().Be(enablement.IsActive);
            entitlement.IsImmediateEffective.Should().Be(enablement.IsImmediateEffective);
            entitlement.EffectiveUtc.Should().Be(enablement.EffectiveUtc);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestEnablementActivate
        (
            [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(false)] bool isActive,
            [CombinatorialValues(+1)] int initialEffectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool initialIsImmediateEffective,
            [CombinatorialValues(+2)] int effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initEffectiveUtc = nowUtc.AddHours(initialEffectiveUtcOffsetInHours);
            var enablement = new Enablement(isActive).WithEffectiveUtc(initEffectiveUtc, initialIsImmediateEffective);
            var entitlement = new Entitlement(renewal)
            {
                Enablement = enablement,
                Clock = clock,
            };

            var effectiveUtc = nowUtc.AddHours(effectiveUtcOffsetInHours);
            entitlement.Activate(effectiveUtc, isImmediateEffective);
            entitlement.IsActive.Should().Be(enablement.IsActive);
            entitlement.IsImmediateEffective.Should().Be(enablement.IsImmediateEffective);
            entitlement.EffectiveUtc.Should().Be(enablement.EffectiveUtc);
        }

        // Initializaiton(-5), Expiry(+0), Deadline(+5)
        public static IEnumerable<(int DeactivationUtcOffsetInHours, int ActivationUtcOffsetInHours, int ExpectedExpiryUtcOffsetInHours)> ExpiryEnablementTimeline
            =>
            [
                // Initialized(-5) => Deactivated(-3) => Activated(-2) => Expired(+0) => Dead(+5)
                // Overdue(Enablement) = -3, Overdue(Expiry) = 0
                (
                    DeactivationUtcOffsetInHours: -3,
                    ActivationUtcOffsetInHours: -2,
                    ExpectedExpiryUtcOffsetInHours: +1
                ),

                // Initialized(-5) => Expired(+0) => Deactivated(+2) => Activated(+3) => Dead(+5)
                // Overdue(Enablement) = +2, Overdue(Expiry) = +2
                (
                    DeactivationUtcOffsetInHours: +2,
                    ActivationUtcOffsetInHours: +3,
                    ExpectedExpiryUtcOffsetInHours: +1
                ),

                // Initialized(-5) => Expired(+0) => Dead(+5) => Deactivated(+6) => Activated(+7)
                // Overdue(Enablement) = +6, Overdue(Expiry) = +5
                (
                    DeactivationUtcOffsetInHours: +6,
                    ActivationUtcOffsetInHours: +7,
                    ExpectedExpiryUtcOffsetInHours: +2
                ),

                // Initialized(-5) => Deactivated(-1) => Expired(+0) => Dead(+5) => Activated(+6)
                // Overdue(Enablement) = -1, Overdue(Expiry) = 0
                (
                    DeactivationUtcOffsetInHours: -1,
                    ActivationUtcOffsetInHours: +6,
                    ExpectedExpiryUtcOffsetInHours: +7
                ),

                // Initialized(-5) => Deactivated(-1) => Expired(+0) => Activated(+1) => Dead(+5)
                // Overdue(Enablement) = -1, Overdue(Expiry) = 0
                (
                    DeactivationUtcOffsetInHours: -1,
                    ActivationUtcOffsetInHours: +1,
                    ExpectedExpiryUtcOffsetInHours: +2
                ),

                // Initialized(-5) => Expired(+0) => Deactivated(+1) => Dead(+5) => Activated(+6) 
                // Overdue(Enablement) = +1, Overdue(Expiry) = +1
                (
                    DeactivationUtcOffsetInHours: +1,
                    ActivationUtcOffsetInHours: +6,
                    ExpectedExpiryUtcOffsetInHours: +5
                ),
            ];

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestEnablementActivateWithAutoRenewal
        (
            [CombinatorialValues(AutoRenewalMode.All)] AutoRenewalMode renewal,
            [CombinatorialValues(-5)] int nowUtcOffsetInHours,
            [CombinatorialValues(true)] bool isActive,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string expiryUtcString,
            [CombinatorialValues(+5)] int gracePeriodInHours,
            [CombinatorialMemberData(nameof(ExpiryEnablementTimeline))]
            (
                int DeactivationUtcOffsetInHours,
                int ActivationUtcOffsetInHours,
                int ExpectedExpiryUtcOffsetInHours
            ) timeline,
            [CombinatorialValues(true, false)] bool isImmediateDeactivated,
            [CombinatorialValues(true, false)] bool isImmediateActivated
        )
        {
            var expiryUtc = InstantPattern.General.Parse(expiryUtcString).Value.ToDateTimeUtc();
            var nowUtc = expiryUtc.AddHours(nowUtcOffsetInHours);
            var clock = new FakeClock(nowUtc.ToInstant());
            var grace = TimeSpan.FromHours(gracePeriodInHours);
            var expiry = new Expiry(expiryUtc, grace);
            var enablement = new Enablement(isActive).WithEffectiveUtc(nowUtc, true);
            var entitlement = new Entitlement(renewal)
            {
                Enablement = enablement,
                Expiry = expiry,
                Clock = clock,
            };

            var deactivationUtc = expiryUtc.AddHours(timeline.DeactivationUtcOffsetInHours);
            clock.Reset(deactivationUtc.ToInstant());
            entitlement.Deactivate(deactivationUtc, isImmediateDeactivated);
            var activationUtc = expiryUtc.AddHours(timeline.ActivationUtcOffsetInHours);
            clock.Reset(activationUtc.ToInstant());
            entitlement.Activate(activationUtc, isImmediateActivated);

            entitlement.IsActive.Should().Be(enablement.IsActive);
            entitlement.IsImmediateEffective.Should().Be(isImmediateActivated);
            entitlement.EffectiveUtc.Should().Be(activationUtc);
            var expectedExpiryUtc = expiryUtc.AddHours(timeline.ExpectedExpiryUtcOffsetInHours);
            entitlement.ExpiryUtc.Should().Be(expectedExpiryUtc);
       }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestEnablementDeactivate
        (
            [CombinatorialValues(AutoRenewalMode.Default)] AutoRenewalMode renewal,
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString,
            [CombinatorialValues(true)] bool isActive,
            [CombinatorialValues(+1)] int initialEffectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool initialIsImmediateEffective,
            [CombinatorialValues(+2)] int effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initEffectiveUtc = nowUtc.AddHours(initialEffectiveUtcOffsetInHours);
            var enablement = new Enablement(isActive).WithEffectiveUtc(initEffectiveUtc, initialIsImmediateEffective);
            var entitlement = new Entitlement(renewal)
            {
                Enablement = enablement,
                Clock = clock,
            };

            var effectiveUtc = nowUtc.AddHours(effectiveUtcOffsetInHours);
            entitlement.Deactivate(effectiveUtc, isImmediateEffective);
            entitlement.IsActive.Should().Be(enablement.IsActive);
            entitlement.IsImmediateEffective.Should().Be(enablement.IsImmediateEffective);
            entitlement.EffectiveUtc.Should().Be(enablement.EffectiveUtc);
        }
    }
}
