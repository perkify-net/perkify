// <copyright file="AutoRenewalMode.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>The auto renewal mode.</summary>
[Flags]
public enum AutoRenewalMode
{
    /// <summary>
    /// No expiry renewal for any balance updating operations.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// Renew expiry time automatically while balance topup.
    /// </summary>
    Topup = 0x0001,

    /// <summary>
    /// Renew expiry time automatically while balance deduction.
    /// </summary>
    Deduct = 0x0002,

    /// <summary>
    /// Renew expiry time automatically while balance adjustment.
    /// </summary>
    Adjust = 0x0004,

    /// <summary>
    /// Renew expiry time automatically for all balance updating operations: Topup, Deduct, and Adjust.
    /// </summary>
    Balance = Topup | Deduct | Adjust,

    /// <summary>
    /// Renew expiry time based on overdue while activating enablement.
    /// </summary>
    Enablement = 0x0010,

    /// <summary>
    /// Default value: Topup and Deduct, Enablement.
    /// </summary>
    Default = Topup | Deduct | Enablement,

    /// <summary>
    /// Renew expiry time for all balance and enablement operations.
    /// </summary>
    All = Balance | Enablement,
}
