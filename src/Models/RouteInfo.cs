using UnityEngine;

namespace GhostMod.Models
{
    /// <summary>
    /// Defines a racing route with start/finish information
    /// </summary>
    public class RouteInfo
    {
        public string RouteName { get; private set; }
        public string SceneName { get; private set; }
        public string StartPointP1Name { get; private set; }
        public string StartPointP2Name { get; private set; }
        public string FinishZoneName { get; private set; }
        public Vector3 FallbackStartP1 { get; private set; }
        public Vector3 FallbackStartP2 { get; private set; }
        public Quaternion FallbackRotation { get; private set; }
        
        /// <summary>
        /// Optional custom finish position. If set, creates a trigger box here instead of using existing zones.
        /// </summary>
        public Vector3? CustomFinishPosition { get; private set; }
        
        /// <summary>
        /// Size of the custom finish trigger box (default 30x10x30)
        /// </summary>
        public Vector3 CustomFinishSize { get; private set; }

        /// <summary>
        /// Constructor without custom finish (uses existing FinishZone objects)
        /// </summary>
        public RouteInfo(string routeName, string sceneName, string startP1, string startP2, string finishZone,
            Vector3 fallbackP1, Vector3 fallbackP2, Quaternion fallbackRotation)
        {
            RouteName = routeName;
            SceneName = sceneName;
            StartPointP1Name = startP1;
            StartPointP2Name = startP2;
            FinishZoneName = finishZone;
            FallbackStartP1 = fallbackP1;
            FallbackStartP2 = fallbackP2;
            FallbackRotation = fallbackRotation;
            CustomFinishPosition = null;
            CustomFinishSize = new Vector3(30f, 10f, 30f);
        }

        /// <summary>
        /// Constructor with custom finish position (creates a trigger box at specified location)
        /// </summary>
        public RouteInfo(string routeName, string sceneName, string startP1, string startP2, string finishZone,
            Vector3 fallbackP1, Vector3 fallbackP2, Quaternion fallbackRotation,
            Vector3 customFinishPos, Vector3 customFinishSize)
        {
            RouteName = routeName;
            SceneName = sceneName;
            StartPointP1Name = startP1;
            StartPointP2Name = startP2;
            FinishZoneName = finishZone;
            FallbackStartP1 = fallbackP1;
            FallbackStartP2 = fallbackP2;
            FallbackRotation = fallbackRotation;
            CustomFinishPosition = customFinishPos;
            CustomFinishSize = customFinishSize;
        }
        
        /// <summary>
        /// Constructor with custom finish position (default size)
        /// </summary>
        public RouteInfo(string routeName, string sceneName, string startP1, string startP2, string finishZone,
            Vector3 fallbackP1, Vector3 fallbackP2, Quaternion fallbackRotation,
            Vector3 customFinishPos)
        {
            RouteName = routeName;
            SceneName = sceneName;
            StartPointP1Name = startP1;
            StartPointP2Name = startP2;
            FinishZoneName = finishZone;
            FallbackStartP1 = fallbackP1;
            FallbackStartP2 = fallbackP2;
            FallbackRotation = fallbackRotation;
            CustomFinishPosition = customFinishPos;
            CustomFinishSize = new Vector3(30f, 10f, 30f);
        }
    }
}