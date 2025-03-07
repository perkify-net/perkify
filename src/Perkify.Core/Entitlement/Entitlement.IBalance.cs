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
        this.balance!.Topup(delta);

        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Topup))
        {
            this.expiry?.Renew();
        }

        return delta;
    }

    /// <inheritdoc/>
    public long Deduct(long delta)
    {
        var result = this.balance!.Deduct(delta);

        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Deduct))
        {
            this.expiry?.Renew();
        }

        return result;
    }

    /// <inheritdoc/>
    public void Adjust(long? incoming, long? outgoing)
    {
        this.balance!.Adjust(incoming, outgoing);
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
        {
            this.expiry?.Renew();
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.balance!.Clear();
        if (this.AutoRenewalMode.HasFlag(AutoRenewalMode.Adjust))
        {
            this.expiry?.Renew();
        }
    }
}
