// <copyright file="AutoRenewalMode.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The auto renewal mode.</summary>
    [Flags]
    public enum AutoRenewalMode
    {
        /// <summary>No expiry renewal for any balance updating operations.</summary>
        None = 0x0000,

        /// <summary>Renew expiry time automatically while balance topup.</summary>
        Topup = 0x0001,

        /// <summary>Renew expiry time automatically while balance deduction.</summary>
        Deduct = 0x0002,

        /// <summary>Renew expiry time automatically while balance adjustment.</summary>
        Adjust = 0x0004,

        /// <summary>Default value: Topup and Deduct.</summary>
        Default = Topup | Deduct,

        /// <summary>Renew expiry time automatically for all balance updating operations: Topup, Deduct, and Adjust.</summary>
        All = Topup | Deduct | Adjust,
    }
}
