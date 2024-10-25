// <copyright file="Expiry.IEligible.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <inheritdoc/>
public partial class Expiry : IEligible
{
    /// <inheritdoc/>
    public virtual bool IsEligible => this.Clock.GetCurrentInstant().ToDateTimeUtc() < this.DeadlineUtc;
}
