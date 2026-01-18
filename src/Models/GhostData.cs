using System;
using System.Collections.Generic;

namespace GhostMod.Models
{
    /// <summary>
    /// Complete ghost recording data
    /// Contains all frames and metadata for a single run
    /// </summary>
    [Serializable]
    public class GhostData
    {
        public string RouteName;
        public string CarName;
        public float TotalTime;
        public long RecordedDate;
        public List<GhostFrame> Frames = new List<GhostFrame>();

        /// <summary>
        /// Format total time as MM:SS.mmm
        /// </summary>
        public string GetTimeString()
        {
            int minutes = (int)(TotalTime / 60);
            float seconds = TotalTime % 60;
            return $"{minutes}:{seconds:00.000}";
        }

        /// <summary>
        /// Get recorded date as DateTime
        /// </summary>
        public DateTime GetRecordedDateTime()
        {
            return new DateTime(RecordedDate);
        }
    }
}
