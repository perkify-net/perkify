namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        const string SkipOrNot = null;

        [Theory, CombinatorialData]
        public void TestCreateEnablement
        (
            [CombinatorialValues(true, false)]bool isActive
        )
        {
            var enablement = new Enablement(isActive);
            enablement.IsActive.Should().Be(isActive);
            enablement.IsImmediateEffective.Should().BeTrue();
            enablement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
            enablement.EffectiveUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
        }

        [Theory, CombinatorialData]
        public void TestCreateEnablementWithFakedClock
        (
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var enablement = new Enablement(isActive) { Clock = clock };
            enablement.IsActive.Should().Be(isActive);
            enablement.IsImmediateEffective.Should().BeTrue();
            enablement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);

            // NOTE: The faked clock is not used for default effective UTC.
            enablement.EffectiveUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
        }

        [Theory, CombinatorialData]
        public void TestCreateEnablementWithEffectiveUtc
        (
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int EffectiveUtcOffset,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var effectiveUtc = nowUtc.AddHours(EffectiveUtcOffset);

            var enablement = new Enablement(isActive) { Clock = clock }.WithEffectiveUtc(effectiveUtc, isImmediateEffective);
            enablement.IsActive.Should().Be(isActive);
            enablement.IsImmediateEffective.Should().Be(isImmediateEffective);
            enablement.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            enablement.EffectiveUtc.Should().Be(effectiveUtc);
        }
    }
}
