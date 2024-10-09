using System.Runtime.InteropServices;

namespace Perkify.Core
{
    /// <summary>
    /// The balance amount with threshold for eligibility.
    /// </summary>
    public class Balance : IEligible, IBalance<Balance>
    {
        #region Factory Methods

        /// <summary>
        /// Create a new balance with threshold.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="threshold"></param>
        public Balance(long threshold)
        {
            this.Incoming = 0;
            this.Outgoing = 0;
            this.Threshold = threshold;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public static Balance Debit() => new Balance(threshold: 0);

        /// <summary>TODO</summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Balance Credit(long threshold)
        {
            if (threshold >= 0)
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold amount must be less than 0");
            return new Balance(threshold);
        }

        /// <summary>TODO</summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        /// See also in IEligible interface.
        /// </summary>
        public bool IsEligible => this.GetBalanceAmount() >= Threshold;

        #endregion

        #region Implements IBalance<T> interface

        /// <summary>See also in IBalance<T> interface.</summary>
        public long Incoming { get; private set; }

        /// <summary>See also in IBalance<T> interface.</summary>
        public long Outgoing { get; private set; }

        /// <summary>See also in IBalance<T> interface.</summary>
        public long Threshold { get; private set; }

        /// <summary>See also in IBalance<T> interface.</summary>
        public void Topup(long delta)
        {
            if (delta < 0)
                throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be greater than 0");

            checked { Incoming += delta; }
        }

        /// <summary>See also in IBalance<T> interface.</summary>
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
            checked { Outgoing += processed; }
            return remaining;
        }

        /// <summary>See also in IBalance<T> interface.</summary>
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

            Incoming = incoming ?? Incoming;
            Outgoing = outgoing ?? Outgoing;
            return this;
        }

        #endregion
    }
}
