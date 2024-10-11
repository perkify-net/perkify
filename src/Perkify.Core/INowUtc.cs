// <copyright file="INowUtc.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>Maintain the current time in UTC.</summary>
    public interface INowUtc
    {
        /// <summary>Gets the current time in UTC.</summary>
        public DateTime NowUtc { get; }
    }
}