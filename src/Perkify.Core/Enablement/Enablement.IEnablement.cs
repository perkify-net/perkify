// <copyright file="Enablement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Enablement : IEnablement
{
    /// <inheritdoc/>
    public bool IsActive { get; private set; } = isActive;

    /// <inheritdoc/>
    public DateTime EffectiveUtc { get; private set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public bool IsImmediateEffective { get; private set; } = true;

    /// <inheritdoc/>
    public void Activate(DateTime? effectiveUtc)
    {
        if (this.IsEligible)
        {
            throw new InvalidOperationException("Already in active state.");
        }

        this.StateChangeExecutor.Execute(EnablementStateOperation.Activate, () =>
        {
            this.EffectiveUtc = effectiveUtc ?? this.Clock.GetCurrentInstant().ToDateTimeUtc();
            this.IsImmediateEffective = effectiveUtc == null;
            if (this.IsImmediateEffective)
            {
                this.IsActive = true;
            }
        });
    }

    /// <inheritdoc/>
    public void Deactivate(DateTime? effectiveUtc)
    {
        if (!this.IsEligible)
        {
            throw new InvalidOperationException("Already in inactive state.");
        }

        this.StateChangeExecutor.Execute(EnablementStateOperation.Deactivate, () =>
        {
            this.EffectiveUtc = effectiveUtc ?? this.Clock.GetCurrentInstant().ToDateTimeUtc();
            this.IsImmediateEffective = effectiveUtc == null;
            if (this.IsImmediateEffective)
            {
                this.IsActive = false;
            }
        });
    }
}
