namespace Perkify.Core.Tests
{
    using FluentAssertions;
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
    }
}
