﻿// <copyright file="EntitlementChainPolicy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// Specifies the policies for entitlement chains.
/// </summary>
[Flags]
public enum EntitlementChainPolicy
{
    /// <summary>
    /// No feature is enabled.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// The flag to indicate if all balance view are calculated based on eligible entitlements only.
    /// </summary>
    EligibleOnlyView = 0x0001,

    /// <summary>
    /// The flag to indicate if auto renewal expiry is enabled (top-up).
    /// </summary>
    WithAutoRenewalExpiry = 0x0002,

    /// <summary>
    /// The flag to indicate if split deduction is allowed (deduction).
    /// </summary>
    SplitDeductionAllowed = 0x0004,

    /// <summary>
    /// The default policy to enable split deduction.
    /// </summary>
    Default = WithAutoRenewalExpiry | SplitDeductionAllowed,

    /// <summary>
    /// The policy to enable all features.
    /// </summary>
    All = EligibleOnlyView | WithAutoRenewalExpiry | SplitDeductionAllowed,
}
