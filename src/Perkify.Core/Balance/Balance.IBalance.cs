// <copyright file="Balance.IBalance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Balance : IBalance
{
    /// <inheritdoc/>
    public long Threshold { get; } = threshold;

    /// <inheritdoc/>
    public BalanceExceedancePolicy BalanceExceedancePolicy { get; } = policy;

    /// <inheritdoc/>
    public BalanceType BalanceType => this.Threshold >= 0 ? BalanceType.Debit : BalanceType.Credit;

    /// <inheritdoc/>
    public long Incoming { get; private set; }

    /// <inheritdoc/>
    public long Outgoing { get; private set; }

    /// <inheritdoc/>
    public long Gross => this.Incoming - this.Outgoing;

    /// <inheritdoc/>
    public long Available => this.Gross >= this.Threshold ? this.Gross - this.Threshold : 0L;

    /// <inheritdoc/>
    public long Overspending => this.Gross >= this.Threshold ? 0L : this.Threshold - this.Gross;

    /// <inheritdoc/>
    public void Topup(long delta)
    {
        if (delta < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be positive or zero.");
        }

        this.StateChangeExecutor.Execute(BalanceStateOperation.Topup, () =>
        {
            checked
            {
                this.Incoming += delta;
            }
        });
    }

    /// <inheritdoc/>
    public long Deduct(long delta)
    {
        if (delta < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be positive or zero.");
        }

        if (!this.IsEligible)
        {
            throw new InvalidOperationException("Ineligible state.");
        }

        var maximum = this.BalanceExceedancePolicy.GetDeductibleAllowance(this.Gross, this.Threshold);
        var processed = delta;
        var remaining = this.BalanceExceedancePolicy.Deduct(ref processed, maximum);
        this.StateChangeExecutor.Execute(BalanceStateOperation.Deduct, () =>
        {
            checked
            {
                this.Outgoing += processed;
            }
        });

        return remaining;
    }

    /// <inheritdoc/>
    public void Adjust(long? incoming, long? outgoing)
    {
        this.StateChangeExecutor.Execute(BalanceStateOperation.Adjust, () =>
        {
            checked
            {
                this.Incoming += incoming ?? 0;
                this.Outgoing += outgoing ?? 0;
            }
        });
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.StateChangeExecutor.Execute(BalanceStateOperation.Adjust, () =>
        {
            this.Incoming = 0;
            this.Outgoing = 0;
        });
    }
}
