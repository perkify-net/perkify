// <copyright file="Balance.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <inheritdoc/>
    public partial class Balance : IEligible
    {
        /// <inheritdoc/>
        public virtual bool IsEligible => this.Gross >= this.Threshold;
    }
}
