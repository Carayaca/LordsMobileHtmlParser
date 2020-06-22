namespace LordsMobile.Core.ProgramOptions
{
    /// <summary>
    /// Migration options.
    /// </summary>
    public interface IMigrationOptions
    {
        /// <summary>
        /// Gets or sets the player name.
        /// </summary>
        public string Player { get; set; }

        /// <summary>
        /// Gets or sets the kingdoms range.
        /// </summary>
        public string KingdomsRange { get; set; }

        /// <summary>
        /// Gets or sets the scrolls threshold.
        /// </summary>
        public int Threshold { get; set; }
    }
}
