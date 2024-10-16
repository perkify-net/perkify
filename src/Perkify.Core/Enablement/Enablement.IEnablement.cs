// <copyright file="Enablement.IEnablement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Enablement : IEnablement
    {
        /// <inheritdoc/>
        public event EventHandler<EnablementStateChangeEventArgs>? StateChanged;

        /// <inheritdoc/>
        public bool IsActive { get; private set; } = isActive;

        /// <inheritdoc/>
        public DateTime EffectiveUtc { get; private set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public bool IsImmediateEffective { get; private set; } = true;

        /// <inheritdoc/>
        public void Activate(DateTime? effectiveUtc, bool isImmediateEffective)
        {
            if (this.IsEligible)
            {
                throw new InvalidOperationException("Already in active state.");
            }

            var previous = new EnablementState(this.IsActive)
            {
                EffictiveUtc = this.EffectiveUtc,
                IsImmediateEffective = this.IsImmediateEffective,
            };
            this.EffectiveUtc = effectiveUtc ?? this.NowUtc;
            this.IsImmediateEffective = isImmediateEffective;
            if (isImmediateEffective)
            {
                this.IsActive = true;
            }

            this.StateChanged?.Invoke(this, new EnablementStateChangeEventArgs(EnablemenStateOperation.Activate)
            {
                From = previous,
                To = new EnablementState(this.IsActive)
                {
                    EffictiveUtc = this.EffectiveUtc,
                    IsImmediateEffective = this.IsImmediateEffective,
                },
            });
        }

        /// <inheritdoc/>
        public void Deactivate(DateTime? effectiveUtc, bool isImmediateEffective)
        {
            if (!this.IsEligible)
            {
                throw new InvalidOperationException("Already in inactive state.");
            }

            var previous = new EnablementState(this.IsActive)
            {
                EffictiveUtc = this.EffectiveUtc,
                IsImmediateEffective = this.IsImmediateEffective,
            };
            this.EffectiveUtc = effectiveUtc ?? this.NowUtc;
            this.IsImmediateEffective = isImmediateEffective;
            if (isImmediateEffective)
            {
                this.IsActive = false;
            }

            this.StateChanged?.Invoke(this, new EnablementStateChangeEventArgs(EnablemenStateOperation.Deactivate)
            {
                From = previous,
                To = new EnablementState(this.IsActive)
                {
                    EffictiveUtc = this.EffectiveUtc,
                    IsImmediateEffective = this.IsImmediateEffective,
                },
            });
        }
    }
}
