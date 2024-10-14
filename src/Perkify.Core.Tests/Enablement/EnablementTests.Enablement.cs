namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsActive
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(null, +1, 0, -1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            var expected = deactivationUtcOffsetInHours == null;
            enablement.IsActive.Should().Be(expected);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeactivate
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(+1, 0, -1, null)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(null) { Clock = clock };
            enablement.IsActive.Should().BeTrue();

            enablement.Deactivate(deactivationUtc);
            enablement.IsActive.Should().BeFalse();
            enablement.DeactivationUtc.HasValue.Should().BeTrue();
            enablement.DeactivationUtc.Should().Be(deactivationUtc ?? nowUtc);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestDeactivateWithIncorrectState
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(+1, 0, -1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            enablement.IsActive.Should().BeFalse();

            var action = new Action(() => enablement.Deactivate(nowUtc));
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Already in inactive state.");
            enablement.IsActive.Should().BeFalse();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestActivate
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(+1, 0, -1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            enablement.IsActive.Should().BeFalse();

            enablement.Activate(nowUtc);
            enablement.IsActive.Should().BeTrue();
            enablement.DeactivationUtc.HasValue.Should().BeFalse();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestActivateWithIncorrectState
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var enablement = new Enablement(null) { Clock = clock };
            enablement.IsActive.Should().BeTrue();

            var action = new Action(() => enablement.Activate(nowUtc));
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Already in active state.");
            enablement.IsActive.Should().BeTrue();
        }
    }
}
