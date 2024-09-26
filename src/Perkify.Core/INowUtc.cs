namespace Perkify.Core
{
    /// <summary>
    /// Maintain the current time in UTC.
    /// </summary>
    public interface INowUtc
    {
        /// <summary>The current time in UTC.</summary>
        public DateTime NowUtc { get; }
    }
}