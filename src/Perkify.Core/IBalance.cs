namespace Perkify.Core
{
    /// <summary>
    /// The balance type: debit or credit.
    /// </summary>
    public enum BalanceType
    {
        // Threshold = 0, Amount >= Threshold
        Debit = 0,

        // Threshold < 0, Amount >= Threshold 
        Credit = 1,
    }

    /// <summary>
    /// The interface to maintain a balance amount with threshold for eligibility.
    /// </summary>
    public interface IBalance<T> where T : IBalance<T>
    {
        /// <summary>
        /// All incoming revenue to the balance.
        /// </summary>
        public long Incoming { get; }

        /// <summary>
        /// All outgoing expenses from the balance.
        /// </summary>
        public long Outgoing { get; }

        /// <summary>
        /// The threshold amount for the balance.
        /// </summary>
        public long Threshold { get; }

        /// <summary>
        /// Topup the balance with incoming revenue.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T Topup(long delta);

        /// <summary>
        /// Spend the balance with outgoing expenses.
        /// </summary>
        /// <param name="delta">The amount to spend from the balance.</param>
        /// <param name="overspending">The flag to allow over-spending the balance</param>
        /// <returns>The balance after spending the amount.</returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Raised when the amount is less than 0 or overspending.</exception>
        public T Deduct(long delta, bool overspending = true);

        /// <summary>
        /// Adjust the balance with incoming and outgoing amounts.
        /// </summary>
        /// <param name="incoming">The incoming amount to adjust the balance.</param>
        /// <param name="outgoing">The outgoing amount to adjust the balance.</param>
        /// <returns>The adjusted balance with incoming and outgoing amounts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The incoming amount must be null or greater than or equal to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The outgoing amount must be null or greater than or equal to 0.</exception>
        public T Adjust(long? incoming, long? outgoing);
    }

    public static class BalanceExtensions
    {
        /// <summary>
        /// The current balance amount.
        /// </summary>
        public static long GetBalanceAmount<T>(this T balance) where T: IBalance<T>
            => balance.Incoming - balance.Outgoing;

        /// <summary>
        /// The balance type: debit or credit.
        /// </summary>
        public static BalanceType GetBalanceType<T>(this T balance) where T: IBalance<T>
            => balance.Threshold >= 0 ? BalanceType.Debit : BalanceType.Credit;

        /// <summary>
        /// The over spending amount.
        /// </summary>
        public static long GetOverSpendingAmount<T>(this T balance) where T : IBalance<T>
            => balance.GetBalanceAmount() >= balance.Threshold ? 0 : balance.Threshold - balance.GetBalanceAmount();

        /// <summary>
        /// Clear the balance with zero amount.
        /// </summary>
        /// <returns>The cleared balance with zero amount.</returns>
        public static T Clear<T>(this T balance) where T : IBalance<T>
            => balance.Adjust(0, 0);
    }
}
