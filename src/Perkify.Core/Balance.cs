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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Balance Credit(long threshold)
        {
            if (threshold >= 0)
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold amount must be less than 0");
            return new Balance(threshold);
        }

        #endregion

        #region Implements IEligible interface

        /// <summary>
        /// See also in IEligible interface.
        /// </summary>
        public bool IsEligible => this.GetBalanceAmount() >= Threshold;

        #endregion

        #region Implements IBalance<T> interface

        /// <summary>
        /// All incoming revenue to the balance.
        /// </summary>
        public long Incoming { get; private set; }

        /// <summary>
        /// All outgoing expenses from the balance.
        /// </summary>
        public long Outgoing { get; private set; }

        /// <summary>
        /// The threshold amount for the balance.
        /// </summary>
        public long Threshold { get; private set; }

        /// <summary>
        /// Topup the balance with incoming revenue.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Balance Topup(long delta)
        {
            if (delta < 0)
                throw new ArgumentOutOfRangeException(nameof(delta), "Amount must be greater than 0");

            checked { Incoming += delta; }
            return this;
        }

        /// <summary>
        /// Spend the balance with outgoing expenses.
        /// </summary>
        /// <param name="delta">The amount to spend from the balance.</param>
        /// <param name="overspending">The flag to allow over-spending the balance</param>
        /// <returns>The balance after spending the amount.</returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Raised when the amount is less than 0 or overspending.</exception>
        public Balance Spend(long delta, bool overspending = true)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "The Amount must be greater than 0.");
            }

            if (!this.IsEligible)
            {
                throw new InvalidOperationException("Ineligible state.");
            }

            if (!overspending && this.GetBalanceAmount() - delta < Threshold)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "The amount is over-spending.");
            }

            checked { Outgoing += delta; }
            return this;
        }

        /// <summary>
        /// Adjust the balance with incoming and outgoing amounts.
        /// </summary>
        /// <param name="incoming">The incoming amount to adjust the balance.</param>
        /// <param name="outgoing">The outgoing amount to adjust the balance.</param>
        /// <returns>The adjusted balance with incoming and outgoing amounts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The incoming amount must be null or greater than or equal to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The outgoing amount must be null or greater than or equal to 0.</exception>
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
