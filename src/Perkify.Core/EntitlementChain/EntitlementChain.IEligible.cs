// <copyright file="EntitlementChain.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class EntitlementChain : IEligible
{
    /// <inheritdoc/>
    public virtual bool IsEligible => this.entitlements.Any(entitlement => entitlement.IsEligible);
}
