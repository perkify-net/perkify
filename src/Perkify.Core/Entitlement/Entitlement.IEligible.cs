// <copyright file="Entitlement.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Entitlement : IEligible
{
    /// <inheritdoc/>
    public virtual bool IsEligible =>
        (this.balance?.IsEligible ?? true)
        && (this.expiry?.IsEligible ?? true)
        && (this.enablement?.IsEligible ?? true)
        && (this.Prerequesite?.IsEligible ?? true);
}
