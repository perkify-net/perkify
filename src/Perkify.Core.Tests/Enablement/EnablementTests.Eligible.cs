namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        [Theory, CombinatorialData]
        public void TestIsEligibleWithImmediateEffectiveUtc
        (
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int EffectiveUtcOffset,
            [CombinatorialValues(true)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var effectiveUtc = nowUtc.AddHours(EffectiveUtcOffset);

            var enablement = new Enablement(isActive) { Clock = clock }.WithEffectiveUtc(effectiveUtc, isImmediateEffective);
            enablement.IsEligible.Should().Be(isActive);
        }

        [Theory, CombinatorialData]
        public void TestIsEligibleWithEffectiveUtcInPast
        (
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0)] int EffectiveUtcOffset,
            [CombinatorialValues(false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var effectiveUtc = nowUtc.AddHours(EffectiveUtcOffset);

            var enablement = new Enablement(isActive) { Clock = clock }.WithEffectiveUtc(effectiveUtc, isImmediateEffective);
            enablement.IsEligible.Should().Be(!isActive);
        }

        [Theory, CombinatorialData]
        public void TestIsEligibleWithEffectiveUtcInFuture
        (
            [CombinatorialValues(true, false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(+1)] int EffectiveUtcOffset,
            [CombinatorialValues(false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var effectiveUtc = nowUtc.AddHours(EffectiveUtcOffset);

            var enablement = new Enablement(isActive) { Clock = clock }.WithEffectiveUtc(effectiveUtc, isImmediateEffective);
            enablement.IsEligible.Should().Be(isActive);
        }
    }
}
