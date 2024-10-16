namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeactivate
        (
            [CombinatorialValues(true)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int initialEffiectiveUtcOffsetInHours,
            [CombinatorialValues(true)] bool initialIsImmediateEffective,

            [CombinatorialValues(+1, 0, -1, null)] int? effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective,
            [CombinatorialValues(true, false)] bool isStateChangedEventHooked
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initialEffiectiveUtc = nowUtc.AddHours(initialEffiectiveUtcOffsetInHours);

            var enablement = new Enablement(isActive)
            {
                Clock = clock
            }.WithEffectiveUtc(initialEffiectiveUtc, initialIsImmediateEffective);
            enablement.IsActive.Should().Be(isActive);
            EnablementStateChangeEventArgs? stateChangedEvent = null;
            if (isStateChangedEventHooked)
            {
                enablement.EnablementStateChanged += (sender, e) => { stateChangedEvent = e; };
            }

            var effectiveUtc = effectiveUtcOffsetInHours != null ? nowUtc.AddHours(effectiveUtcOffsetInHours.Value) : (DateTime?)null;
            enablement.Deactivate(effectiveUtc, isImmediateEffective);
            enablement.IsImmediateEffective.Should().Be(isImmediateEffective);
            enablement.EffectiveUtc.Should().Be(effectiveUtc ?? nowUtc);
            enablement.IsActive.Should().Be(isImmediateEffective ? !isActive : isActive);
            if (isStateChangedEventHooked)
            {
                stateChangedEvent.Should().NotBeNull();
                stateChangedEvent!.Operation.Should().Be(EnablemenStateOperation.Deactivate);
                stateChangedEvent!.From.IsActive.Should().Be(isActive);
                stateChangedEvent!.From.EffictiveUtc.Should().Be(initialEffiectiveUtc);
                stateChangedEvent!.From.IsImmediateEffective.Should().Be(initialIsImmediateEffective);
                stateChangedEvent!.To.IsActive.Should().Be(enablement.IsActive);
                stateChangedEvent!.To.EffictiveUtc.Should().Be(enablement.EffectiveUtc);
                stateChangedEvent!.To.IsImmediateEffective.Should().Be(enablement.IsImmediateEffective);
            }
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeactivateWithIncorrectState
        (
            [CombinatorialValues(false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int initialEffiectiveUtcOffsetInHours,
            [CombinatorialValues(true)] bool initialIsImmediateEffective,

            [CombinatorialValues(+1, 0, -1, null)] int? effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initialEffiectiveUtc = nowUtc.AddHours(initialEffiectiveUtcOffsetInHours);

            var enablement = new Enablement(isActive)
            {
                Clock = clock
            }.WithEffectiveUtc(initialEffiectiveUtc, initialIsImmediateEffective);
            enablement.IsActive.Should().Be(isActive);

            var effectiveUtc = effectiveUtcOffsetInHours != null ? nowUtc.AddHours(effectiveUtcOffsetInHours.Value) : (DateTime?)null;
            var action = () => enablement.Deactivate(effectiveUtc, isImmediateEffective);
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Already in inactive state.");
            enablement.IsActive.Should().Be(isActive);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestActivate
        (
            [CombinatorialValues(false)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int initialEffiectiveUtcOffsetInHours,
            [CombinatorialValues(true)] bool initialIsImmediateEffective,

            [CombinatorialValues(+1, 0, -1, null)] int? effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective,
            [CombinatorialValues(true, false)] bool isStateChangedEventHooked
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initialEffiectiveUtc = nowUtc.AddHours(initialEffiectiveUtcOffsetInHours);

            var enablement = new Enablement(isActive)
            {
                Clock = clock
            }.WithEffectiveUtc(initialEffiectiveUtc, initialIsImmediateEffective);
            enablement.IsActive.Should().Be(isActive);
            EnablementStateChangeEventArgs? stateChangedEvent = null;
            if (isStateChangedEventHooked)
            {
                enablement.EnablementStateChanged += (sender, e) => { stateChangedEvent = e; };
            }

            var effectiveUtc = effectiveUtcOffsetInHours != null ? nowUtc.AddHours(effectiveUtcOffsetInHours.Value) : (DateTime?)null;
            enablement.Activate(effectiveUtc, isImmediateEffective);
            enablement.IsImmediateEffective.Should().Be(isImmediateEffective);
            enablement.EffectiveUtc.Should().Be(effectiveUtc ?? nowUtc);
            enablement.IsActive.Should().Be(isImmediateEffective ? !isActive : isActive);
            if (isStateChangedEventHooked)
            {
                stateChangedEvent.Should().NotBeNull();
                stateChangedEvent!.Operation.Should().Be(EnablemenStateOperation.Activate);
                stateChangedEvent!.From.IsActive.Should().Be(isActive);
                stateChangedEvent!.From.EffictiveUtc.Should().Be(initialEffiectiveUtc);
                stateChangedEvent!.From.IsImmediateEffective.Should().Be(initialIsImmediateEffective);
                stateChangedEvent!.To.IsActive.Should().Be(enablement.IsActive);
                stateChangedEvent!.To.EffictiveUtc.Should().Be(enablement.EffectiveUtc);
                stateChangedEvent!.To.IsImmediateEffective.Should().Be(enablement.IsImmediateEffective);
            }
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestActivateWithIncorrectState
        (
            [CombinatorialValues(true)] bool isActive,
            [CombinatorialValues("2024-10-15T16:00:00Z")] string nowUtcString,
            [CombinatorialValues(-1, 0, +1)] int initialEffiectiveUtcOffsetInHours,
            [CombinatorialValues(true)] bool initialIsImmediateEffective,

            [CombinatorialValues(+1, 0, -1, null)] int? effectiveUtcOffsetInHours,
            [CombinatorialValues(true, false)] bool isImmediateEffective
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var initialEffiectiveUtc = nowUtc.AddHours(initialEffiectiveUtcOffsetInHours);

            var enablement = new Enablement(isActive)
            {
                Clock = clock
            }.WithEffectiveUtc(initialEffiectiveUtc, initialIsImmediateEffective);
            enablement.IsActive.Should().Be(isActive);

            var effectiveUtc = effectiveUtcOffsetInHours != null ? nowUtc.AddHours(effectiveUtcOffsetInHours.Value) : (DateTime?)null;
            var action = () => enablement.Activate(effectiveUtc, isImmediateEffective);
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Already in active state.");
            enablement.IsActive.Should().Be(isActive);
        }
    }
}
