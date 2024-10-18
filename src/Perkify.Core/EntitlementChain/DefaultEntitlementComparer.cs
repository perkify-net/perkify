// <copyright file="DefaultEntitlementComparer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a default comparison for <see cref="Entitlement"/> objects.
    /// </summary>
    public class DefaultEntitlementComparer : IComparer<Entitlement>
    {
        /// <summary>
        /// Compares two Entitlement objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first Entitlement to compare.</param>
        /// <param name="y">The second Entitlement to compare.</param>
        /// <returns>A signed integer that indicates the relative values of x and y.</returns>
        public int Compare(Entitlement? x, Entitlement? y)
            => DateTime.Compare(
                t1: x?.ExpiryUtc ?? throw new System.ArgumentNullException(nameof(x)),
                t2: y?.ExpiryUtc ?? throw new System.ArgumentNullException(nameof(y)));
    }
}
