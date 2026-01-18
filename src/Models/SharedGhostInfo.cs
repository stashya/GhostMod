namespace GhostMod.Models
{
    /// <summary>
    /// Metadata about a shared ghost file from another player
    /// Used for displaying in the menu without loading full ghost data
    /// </summary>
    public class SharedGhostInfo
    {
        /// <summary>
        /// Full path to the ghost file
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Player name (extracted from filename)
        /// </summary>
        public string PlayerName;
        
        /// <summary>
        /// Route key (e.g., "Akina_Downhill")
        /// </summary>
        public string RouteKey;
        
        /// <summary>
        /// Total time in seconds
        /// </summary>
        public float TotalTime;
        
        /// <summary>
        /// Formatted time string (MM:SS.mmm)
        /// </summary>
        public string TimeString;
        
        /// <summary>
        /// Whether the ghost file passed validation
        /// </summary>
        public bool IsValid;
    }
}
