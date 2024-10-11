// <copyright file="Balance.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Perkify.Core
{
    /// <summary>The balance amount with threshold for eligibility.</summary>
    public class Balance : IEligible, IBalance<Balance>
    {
        #region Factory Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="Balance"/> class.Create a new balance with threshold.</summary>
        /// <param name="threshold">The threshold amount for the balance.</param>
        public Balance(long threshold)
        {
            this.Incoming = 0;
            this.Outgoing = 0;
            this.Threshold = threshold;
        }

        /// <summary>
        /// Creates a new balance with a threshold of 0.
        /// </summary>
        /// <returns>A new instance of the <see cref="Balance"/> class.</returns>
        public static Balance Debit() => new (threshold: 0);

        /// <summary>TODO.</summary>
        /// <param name="threshold">The threshold amount for the balance.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the threshold amount is greater than or equal to 0.</exception>
        /// <returns></returns>
        public static Balance Credit(long threshold)
        {
            if (threshold >= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold amount must be less than 0");
            }

            return new Balance(threshold);
        }

        /// <summary>
        /// Sets the incoming and outgoing amounts for the balance.
        /// </summary>
        /// <param name="incoming">The incoming amount to set.</param>
        /// <param name="outgoing">The outgoing amount to set.</param>
        /// <returns>The updated balance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the incoming amount is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the outgoing amount is less than 0.</exception>
        public Balance WithBalance(long incoming, long outgoing)
        {
            if (incoming < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(incoming), "Amount must be zero or positive");
            }

            if (outgoing < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outgoing), "Amount must be zero or positive");
            }

            this.Incoming = incoming;
            this.Outgoing = outgoing;
            return this;
        }

        #endregion

        #region Implements IEligible interface

        /// <summary>
        /// Gets a value indicating whether see also in IEligible interface.
        /// </summary>
        public bool IsEligible => this.GetBalanceAmount() >= this.Threshold;

        #endregion

        #region Implements IBalance<T> interface

        /// <summary>Gets see also in `IBalance&lt;T&gt;` interface.</summary>
        public long Incoming { get; private set; }

        /// <summary>Gets see also in `IBalance&lt;T&gt;` interface.</summary>
        public long Outgoing { get; private set; }

        /// <summary>Gets see also in `IBalance&lt;T&gt;` interface.</summary>
        public long Threshold { get; private set; }

        /// <summary>See also in `IBalance&lt;T&gt;` interface.</summary>
        /// <param name="delta">The amount to be added to the balance.</param>
        public void Topup(long delta)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be greater than 0");
            }

            checked
            {
                this.Incoming += delta;
            }
        }

        /// <summary>See also in `IBalance&lt;T&gt;` interface.</summary>
        /// <param name="delta">The amount to be deducted.</param>
        /// <param name="policy">The policy to apply if the balance exceeds the threshold.</param>
        /// <returns>The remaining amount after deduction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the delta is less than 0.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the balance is ineligible for deduction.</exception>
        public long Deduct(long delta, BalanceExceedancePolicy policy = BalanceExceedancePolicy.Reject)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "The Amount must be greater than 0.");
            }

            if (!this.IsEligible)
            {
                throw new InvalidOperationException("Ineligible state.");
            }

            var maximum = this.GetMaxDeductibleAmount(policy);
            var processed = delta;
            var remaining = policy.Deduct(ref processed, maximum);
            checked
            {
                this.Outgoing += processed;
            }

            return remaining;
        }

        /// <summary>
        /// Adjusts the balance with the specified incoming and outgoing amounts.
        /// </summary>
        /// <param name="incoming">The incoming amount to set. Must be null or greater than or equal to 0.</param>
        /// <param name="outgoing">The outgoing amount to set. Must be null or greater than or equal to 0.</param>
        /// <returns>The adjusted balance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the incoming or outgoing amount is less than 0.</exception>
        public Balance Adjust(long? incoming, long? outgoing)
        {
            if (incoming.HasValue && incoming.Value < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(incoming), "The incoming/outgoing amount must be null or greater than or equal to 0");
            }

            if (outgoing.HasValue && outgoing.Value < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(outgoing), "The incoming/outgoing amount must be null or greater than or equal to 0");
            }

            this.Incoming = incoming ?? this.Incoming;
            this.Outgoing = outgoing ?? this.Outgoing;
            return this;
        }

        #endregion
    }
}
