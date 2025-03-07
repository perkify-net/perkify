// <copyright file="Entitlement.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Entitlement : IBalance
{
    /// <inheritdoc/>
    public long Threshold
        => this.balance!.Threshold;

    /// <inheritdoc/>
    public BalanceExceedancePolicy BalanceExceedancePolicy
        => this.balance!.BalanceExceedancePolicy;

    /// <inheritdoc/>
    public BalanceType BalanceType
        => this.balance!.BalanceType;

    /// <inheritdoc/>
    public long Incoming
        => this.balance!.Incoming;

    /// <inheritdoc/>
    public long Outgoing
        => this.balance!.Outgoing;

    /// <inheritdoc/>
    public long Gross
        => this.balance!.Gross;

    /// <inheritdoc/>
    public long Available
        => this.balance!.Available;

    /// <inheritdoc/>
    public long Overspending
        => this.balance!.Overspending;

    /// <inheritdoc/>
    public long Topup(long delta)
    {
        this.CheckBalanceInitialized();

        var nowUtc = this.Clock.GetCurrentInstant().ToDateTimeUtc();
        var allowed = this.IncomeBudget.Verify(nowUtc, delta, precheck: true);
        var result = this.balance!.Topup(allowed);
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
        {
            this.expiry?.Renew();
        }

        this.IncomeBudget.Verify(nowUtc, result, precheck: false);
        return result;
    }

    /// <inheritdoc/>
    public long Deduct(long delta)
    {
        this.CheckBalanceInitialized();

        var nowUtc = this.Clock.GetCurrentInstant().ToDateTimeUtc();
        var allowed = this.OutgoingBudget.Verify(nowUtc, delta, precheck: true);
        var result = this.balance!.Deduct(allowed);
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
        {
            this.expiry?.Renew();
        }

        this.OutgoingBudget.Verify(nowUtc, result, precheck: false);
        return result;
    }

    /// <inheritdoc/>
    public void Adjust(long? incoming, long? outgoing)
    {
        this.CheckBalanceInitialized();
        this.balance!.Adjust(incoming, outgoing);
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
        {
            this.expiry?.Renew();
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.CheckBalanceInitialized();
        this.balance!.Clear();
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
        {
            this.expiry?.Renew();
        }
    }

    private void CheckBalanceInitialized()
    {
        // TODO: Fix if-throw pattern
        if (this.balance == null)
        {
            throw new InvalidOperationException("Balance is not initialized");
        }
    }
}
