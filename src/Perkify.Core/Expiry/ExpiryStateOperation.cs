// <copyright file="ExpiryStateOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>
    /// Represents the operations that can be performed on an expiry state.
    /// </summary>
    public enum ExpiryStateOperation
    {
        /// <summary>
        /// Renew the expiry state.
        /// </summary>
        Renew,

        /// <summary>
        /// Adjust the expiry state.
        /// </summary>
        Adjust,

        /// <summary>
        /// Setup the expiry state.
        /// </summary>
        Setup,
    }
}
