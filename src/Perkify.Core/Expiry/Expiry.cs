// <copyright file="Expiry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

using NodaTime;

/// <summary>
/// Expiry time for eligibility.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Expiry"/> class.
/// Create the expiry time for eligibility.
/// </remarks>
/// <param name="expiryUtc">Expiry time in UTC.</param>
/// <param name="clock">Clock instance used to retrieve the current time.</param>
public partial class Expiry(DateTime expiryUtc, IClock? clock = null)
{
    /// <summary>
    /// Gets or sets the clock instance used to retrieve the current time.
    /// </summary>
    public IClock Clock { get; set; } = clock ?? SystemClock.Instance;

    /// <summary>Specify the renewal period.</summary>
    /// <param name="interval">The renewal interval, specified as an ISO 8601 duration string.</param>
    /// <returns>The expiry time with specified renewal period.</returns>
    public Expiry WithRenewal(string interval)
    {
        this.Renewal = new ChronoInterval(interval);
        return this;
    }
}
