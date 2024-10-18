// <copyright file="Enablement.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Enablement : IEligible
    {
        /// <inheritdoc/>
        public virtual bool IsEligible =>
            this.IsImmediateEffective || this.Clock.GetCurrentInstant().ToDateTimeUtc() < this.EffectiveUtc
                ? this.IsActive
                : !this.IsActive;
    }
}
