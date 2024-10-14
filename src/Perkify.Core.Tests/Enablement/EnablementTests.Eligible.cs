namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsEligible
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(null, +1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            enablement.IsEligible.Should().BeTrue();
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestIsIneligible
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(0, -1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            enablement.IsEligible.Should().BeFalse();
        }
    }
}
