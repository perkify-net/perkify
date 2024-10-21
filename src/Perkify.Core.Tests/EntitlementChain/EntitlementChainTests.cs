namespace Perkify.Core.Tests
{
    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;

    public partial class EntitlementChainTests
    {
        [Fact]
        public void TestCreateChain()
        {
            var chain = new EntitlementChain();
            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
            chain.Factory.Should().NotBeNull();
            chain.Factory.Should().Be(EntitlementChain.DefaultEntitlementFactory);
            chain.Comparer.Should().NotBeNull();
            chain.Comparer.Should().Be(EntitlementChain.DefaultEntitlementComparer);
            chain.Entitlements.Should().HaveCount(0);
        }

        [Fact]
        public void TestCreateChainWithCustomFactoryAndComperer()
        {
            var factory = new Func<long, DateTime?, IClock, Entitlement>((delta, expiryUtc, clock) => null!);
            var comparer = new Mock<IComparer<Entitlement>>().Object;
            var chain = new EntitlementChain()
            {
                Factory = factory,
                Comparer = comparer,
            };
            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
            chain.Factory.Should().NotBeNull();
            chain.Factory.Should().Be(factory);
            chain.Comparer.Should().NotBeNull();
            chain.Comparer.Should().Be(comparer);
            chain.Entitlements.Should().HaveCount(0);
        }

        [Theory, CombinatorialData]
        public void TestCreateChainWithEntitlements
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(200L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                ],
            };

            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            chain.Factory.Should().NotBeNull();
            chain.Factory.Should().Be(EntitlementChain.DefaultEntitlementFactory);
            chain.Comparer.Should().NotBeNull();
            chain.Comparer.Should().Be(EntitlementChain.DefaultEntitlementComparer);

            chain.Entitlements.Should().HaveCount(2);
            chain.Entitlements.First().Incoming.Should().Be(100L);
            chain.Entitlements.First().Outgoing.Should().Be(0L);
            chain.Entitlements.First().ExpiryUtc.Should().Be(nowUtc.AddHours(1));
            chain.Entitlements.First().IsActive.Should().BeTrue();
            chain.Entitlements.First().IsImmediateEffective.Should().BeTrue();

            chain.Entitlements.Last().Incoming.Should().Be(200L);
            chain.Entitlements.Last().Outgoing.Should().Be(0L);
            chain.Entitlements.Last().ExpiryUtc.Should().Be(nowUtc.AddHours(2));
            chain.Entitlements.Last().IsActive.Should().BeTrue();
            chain.Entitlements.Last().IsImmediateEffective.Should().BeTrue();
        }

        [Theory, CombinatorialData]
        public void TestCreateChainWithEntitlementsIsNone
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, clock)
            {
                Entitlements = null!,
            };

            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            chain.Factory.Should().NotBeNull();
            chain.Factory.Should().Be(EntitlementChain.DefaultEntitlementFactory);
            chain.Comparer.Should().NotBeNull();
            chain.Comparer.Should().Be(EntitlementChain.DefaultEntitlementComparer);
            chain.Entitlements.Should().HaveCount(0);
        }

        [Theory, CombinatorialData]
        public void TestCreateChainWithFakedClock
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, null)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(200L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                ],
            }.WithClock(clock);
            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            chain.Entitlements.First().Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            chain.Entitlements.Last().Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
        }

        [Theory, CombinatorialData]
        public void TestCreateChainWithSystemClock
        (
            [CombinatorialValues("2024-10-09T15:00:00Z")] string nowUtcString
        )
        {
            var nowUtc = InstantPattern.General.Parse(nowUtcString).Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(200L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                ],
            }.WithClock(null);

            chain.EntitlementChainPolicy.Should().Be(EntitlementChainPolicy.Default);
            chain.Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
            chain.Entitlements.First().Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
            chain.Entitlements.Last().Clock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(1000));
        }
    }
}
