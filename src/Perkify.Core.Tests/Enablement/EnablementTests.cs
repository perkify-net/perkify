namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EnablementTests
    {
        const string SkipOrNot = null;

        [Theory(Skip = SkipOrNot)]
        [InlineData(null)]
        [InlineData("2021-01-01T00:00:00Z")]
        public void TestCreateEnablement(string? deactivationUtcString)
        {
            var deactivationUtc = deactivationUtcString != null ? InstantPattern.General.Parse(deactivationUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc);
            enablement.DeactivationUtc.Should().Be(deactivationUtc);
        }

        [Theory(Skip = SkipOrNot), CombinatorialData]
        public void TestCreateEnablementWithFakedClock
        (
            [CombinatorialValues("2021-01-01T00:00:00Z")] string nowUtcString,
            [CombinatorialValues(null, -1, 0, +1)] int? deactivationUtcOffsetInHours
        )
        {
            var nowUtc = nowUtcString != null ? InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc() : (DateTime?)null;
            var clock = new FakeClock(nowUtc!.Value.ToInstant());
            var deactivationUtc = deactivationUtcOffsetInHours.HasValue ? nowUtc!.Value.AddHours(deactivationUtcOffsetInHours.Value) : (DateTime?)null;
            var enablement = new Enablement(deactivationUtc) { Clock = clock };
            enablement.DeactivationUtc.Should().Be(deactivationUtc);
            if(nowUtc != null)
                enablement.NowUtc.Should().Be(nowUtc);
            else
                enablement.NowUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
        }
    }
}
