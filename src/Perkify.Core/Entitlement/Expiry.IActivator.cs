// <copyright file="Expiry.IActivator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /*
             /// <inheritdoc/>
        public bool IsEligible => (this.DeactivationUtc, this.NowUtc, this.DeadlineUtc) switch
        {
            // Not suspended
            (null, DateTime n, DateTime d) => n < d,

            // Already suspended
            (DateTime s, DateTime n, DateTime d) when s <= n => false,

            // Will suspend in future
            (DateTime s, DateTime n, DateTime d) when s > n => n < d,

            // Default
            _ => throw new InvalidOperationException("Invalid state.")
        };

        /// <summary>Specify the suspension time.</summary>
        /// <param name="suspensionUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        public Expiry WithSuspensionUtc(DateTime suspensionUtc)
        {
            this.suspensionUtc = suspensionUtc < this.GetDeadlineUtc() ? suspensionUtc : this.GetDeadlineUtc();
            return this;
        }

        /// <summary>Specify the suspension time.</summary>
        /// <param name="suspensionUtc">The suspension time in UTC.</param>
        /// <returns>The expiry time after suspension.</returns>
        public Expiry WithSuspensionUtc(DateTime suspensionUtc)
        {
            this.suspensionUtc = suspensionUtc < this.GetDeadlineUtc() ? suspensionUtc : this.GetDeadlineUtc();
            return this;
        }


    using NodaTime;

    /// <inheritdoc/>
    public partial class Expiry : IActivator<Expiry>
    {
        /// <inheritdoc/>
        public DateTime? DeactivationUtc { get; private set; }

        /// <inheritdoc/>
        public bool IsActive => !this.DeactivationUtc.HasValue;

        /// <inheritdoc/>
        public Expiry Deactivate(DateTime? suspensionUtc = null)
        {
            // Keep idempotent if resubmitting suspending requests.
            if (this.DeactivationUtc.HasValue)
            {
                return this;
            }

            suspensionUtc ??= this.ExpiryUtc;
            var deadlineUtc = this.GetDeadlineUtc();
            this.suspensionUtc = suspensionUtc < deadlineUtc ? suspensionUtc : deadlineUtc;
            return this;
        }

        /// <inheritdoc/>
        public Expiry Activate(DateTime? resumptionUtc = null, bool extended = false)
        {
            if (!this.suspensionUtc.HasValue)
            {
                return this;
            }

            var finalResumptionUtc = resumptionUtc ??= this.NowUtc;
            if (finalResumptionUtc < this.suspensionUtc.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(resumptionUtc), "Resume time must be greater than suspend time.");
            }

            if (extended)
            {
                this.ExpiryUtc = finalResumptionUtc + this.Remaining;
            }

            this.suspensionUtc = null;
            return this;
        }
    }
    */
}
