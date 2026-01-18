using UnityEngine;

namespace GhostMod.Models
{
    /// <summary>
    /// Defines a racing route with start/finish points
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// Display name for the route
        /// </summary>
        public string RouteName;
        
        /// <summary>
        /// Unity scene name where this route exists
        /// </summary>
        public string SceneName;
        
        /// <summary>
        /// Name of the P1 starting point GameObject (first run / challenger position)
        /// </summary>
        public string StartPointP1Name;
        
        /// <summary>
        /// Name of the P2 starting point GameObject (ghost position when challenging)
        /// </summary>
        public string StartPointP2Name;
        
        /// <summary>
        /// Name of the finish zone GameObject
        /// </summary>
        public string FinishZoneName;
        
        /// <summary>
        /// Fallback position for P1 if GameObject not found
        /// </summary>
        public Vector3 FallbackStartP1;
        
        /// <summary>
        /// Fallback position for P2 if GameObject not found
        /// </summary>
        public Vector3 FallbackStartP2;
        
        /// <summary>
        /// Fallback rotation if start point not found
        /// </summary>
        public Quaternion FallbackRotation;

        public RouteInfo(string name, string scene, string p1, string p2, string finish,
            Vector3 fbP1, Vector3 fbP2, Quaternion fbRot)
        {
            RouteName = name;
            SceneName = scene;
            StartPointP1Name = p1;
            StartPointP2Name = p2;
            FinishZoneName = finish;
            FallbackStartP1 = fbP1;
            FallbackStartP2 = fbP2;
            FallbackRotation = fbRot;
        }
    }
}
