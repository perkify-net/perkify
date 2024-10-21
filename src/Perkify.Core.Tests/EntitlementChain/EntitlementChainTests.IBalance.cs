namespace Perkify.Core.Tests
{
    using NodaTime.Extensions;
    using NodaTime.Testing;
    using NodaTime.Text;
    using System.Runtime.CompilerServices;

    public partial class EntitlementChainTests
    {
        [Theory]
        [InlineData(EntitlementChainPolicy.None, false, false, false, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.None, false, false, true, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.None, false, true, false, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.None, true, false, false, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, true, false, BalanceExceedancePolicy.Overflow)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, false, true, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, false, true, true, BalanceExceedancePolicy.Overdraft)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, true, true, BalanceExceedancePolicy.Overdraft)]
        public void TestBalanceExceedancePolicy
        (
            EntitlementChainPolicy policy,
            bool isRejectActive, bool isOverflowActive, bool isOverdraftActive,
            BalanceExceedancePolicy expected
        )
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit(BalanceExceedancePolicy.Reject).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(isRejectActive),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit(BalanceExceedancePolicy.Overflow).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(isOverflowActive),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit(BalanceExceedancePolicy.Overdraft).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(3)),
                        Enablement = new Enablement(isOverdraftActive),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(3);
            chain.BalanceExceedancePolicy.Should().Be(expected);
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.None, true, true, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.None, true, false, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.None, false, true, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.None, false, false, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, true, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, false, BalanceType.Debit, 0L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, false, true, BalanceType.Credit, -100L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, false, false, BalanceType.Debit, 0L)]
        public void TestBalanceTypeAndThreshold
        (
            EntitlementChainPolicy policy, 
            bool isFirstActive, bool isLastActive, 
            BalanceType type, long threshold
        )
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(isFirstActive),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(isLastActive),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            chain.BalanceType.Should().Be(type);
            chain.Threshold.Should().Be(threshold);
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.None, 800L, 300L, 500L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, 300L, 100L, 200L)]
        public void TestBalanceGross(EntitlementChainPolicy policy, long incoming, long outgoing, long gross)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(200L, 100L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Debit().WithBalance(500L, 200L),
                        Expiry = new Expiry(nowUtc.AddHours(3)),
                        Enablement = new Enablement(false),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(3);
            chain.Incoming.Should().Be(incoming);
            chain.Outgoing.Should().Be(outgoing);
            chain.Gross.Should().Be(gross);
            chain.Available.Should().Be(gross);
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.None, true, true, 400L)]
        [InlineData(EntitlementChainPolicy.None, true, false, 400L)]
        [InlineData(EntitlementChainPolicy.None, false, true, 400L)]
        [InlineData(EntitlementChainPolicy.None, false, false, 400L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, true, 0L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, true, false, 0L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, false, true, 0L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, false, false, 0L)]
        public void TestBalanceOverspending(EntitlementChainPolicy policy, bool isFirstActive, bool isLastActive, long overspending)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 200L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(isFirstActive),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100).WithBalance(100L, 500L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(isLastActive),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            chain.Available.Should().Be(0L);
            chain.Overspending.Should().Be(overspending);
        }

        [Theory]
        [InlineData(100L)]
        public void TestBalanceTopup(long incoming)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, clock);
            chain.Entitlements.Should().HaveCount(0);
            chain.IsEligible.Should().BeFalse();

            chain.Topup(incoming);
            chain.Entitlements.Should().HaveCount(1);
            chain.Entitlements.First().Incoming.Should().Be(incoming);
            chain.Entitlements.First().Outgoing.Should().Be(0L);
            chain.Entitlements.First().Clock.GetCurrentInstant().ToDateTimeUtc().Should().Be(nowUtc);
            chain.IsEligible.Should().BeTrue();
            chain.Incoming.Should().Be(incoming);
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, 50L, 150L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView | EntitlementChainPolicy.SplitDeductionAllowed, 150L, 50L)]
        public void TestBalanceDeduct(EntitlementChainPolicy policy, long delta, long gross)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(false),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(3)),
                        Enablement = new Enablement(true),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(3);
            chain.Gross.Should().Be(200L);
            var remained = chain.Deduct(delta);
            remained.Should().Be(0L);
            chain.Gross.Should().Be(gross);
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, 150L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView | EntitlementChainPolicy.SplitDeductionAllowed, 150L)]
        public void TestBalanceDeductNotAvailble(EntitlementChainPolicy policy, long delta)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit(BalanceExceedancePolicy.Reject).WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100, BalanceExceedancePolicy.Reject).WithBalance(0L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(true),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            chain.Available.Should().BeGreaterThanOrEqualTo(delta);

            var action = () => _ = chain.Deduct(delta);
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No availble entitlement.");
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView, 150L)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView | EntitlementChainPolicy.SplitDeductionAllowed, 150L)]
        public void TestBalanceDeductWithoutEntitlments(EntitlementChainPolicy policy, long delta)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock);
            chain.Entitlements.Should().HaveCount(0);
            var action = () => _ = chain.Deduct(delta);
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No availble entitlement.");
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, 50L)]
        [InlineData(100L, null)]
        [InlineData(100L, 50L)]
        public void TestBalanceAdjust(long? incoming, long?outgoing)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(EntitlementChainPolicy.Default, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(false),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            var action = () =>chain.Adjust(incoming, outgoing);
            action.Should().Throw<NotSupportedException>().WithMessage("Please adjust specific entitlement or its balance.");
        }

        [Theory]
        [InlineData(EntitlementChainPolicy.None)]
        [InlineData(EntitlementChainPolicy.EligibleOnlyView)]
        public void TestBalanceClear(EntitlementChainPolicy policy)
        {
            var nowUtc = InstantPattern.General.Parse("2024-10-09T15:00:00Z").Value.ToDateTimeUtc();
            var clock = new FakeClock(nowUtc.ToInstant());
            var chain = new EntitlementChain(policy, clock)
            {
                Entitlements =
                [
                    new Entitlement(AutoRenewalMode.None)
                    {
                        Balance = Balance.Debit().WithBalance(100L, 0L),
                        Expiry = new Expiry(nowUtc.AddHours(1)),
                        Enablement = new Enablement(true),
                    },
                    new Entitlement(AutoRenewalMode.None, clock)
                    {
                        Balance = Balance.Credit(-100L),
                        Expiry = new Expiry(nowUtc.AddHours(2)),
                        Enablement = new Enablement(false),
                    },
                ],
            };
            chain.Entitlements.Should().HaveCount(2);
            chain.Clear();
            chain.Entitlements.First().Incoming.Should().Be(0L);
            chain.Entitlements.First().Outgoing.Should().Be(0L);
            chain.Entitlements.Last().Incoming.Should().Be(0L);
            chain.Entitlements.Last().Outgoing.Should().Be(0L);
        }
    }
}
