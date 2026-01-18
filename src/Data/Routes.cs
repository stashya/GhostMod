using System.Collections.Generic;
using UnityEngine;
using GhostMod.Models;

namespace GhostMod.Data
{
    /// <summary>
    /// Static collection of all available racing routes
    /// </summary>
    public static class Routes
    {
        /// <summary>
        /// All defined routes, keyed by route identifier
        /// </summary>
        public static readonly Dictionary<string, RouteInfo> All = new Dictionary<string, RouteInfo>()
        {
            // ========== Akina / Haruna ==========
            { 
                "Akina_Downhill", 
                new RouteInfo(
                    "Akina Downhill", 
                    "HARUNA", 
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(-177f, 476f, -996f), 
                    new Vector3(-182f, 476f, -996f), 
                    Quaternion.Euler(0, 180, 0)
                ) 
            },
            
            { 
                "Akina_Uphill", 
                new RouteInfo(
                    "Akina Uphill", 
                    "HARUNA",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(292f, 117f, 1840f), 
                    new Vector3(287f, 117f, 1840f), 
                    Quaternion.Euler(0, 0, 0)
                ) 
            },

            // ========== Akagi ==========
            { 
                "Akagi_Downhill", 
                new RouteInfo(
                    "Akagi Downhill", 
                    "AKAGI",
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(-50f, 300f, -200f), 
                    new Vector3(-55f, 300f, -200f), 
                    Quaternion.Euler(0, 180, 0)
                ) 
            },
            
            { 
                "Akagi_Uphill", 
                new RouteInfo(
                    "Akagi Uphill", 
                    "AKAGI",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(100f, 50f, 500f), 
                    new Vector3(95f, 50f, 500f), 
                    Quaternion.Euler(0, 0, 0)
                ) 
            },

            // ========== Irohazaka ==========
            { 
                "Irohazaka_Downhill", 
                new RouteInfo(
                    "Irohazaka Downhill", 
                    "IROHAZAKA",
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(0f, 400f, 0f), 
                    new Vector3(-5f, 400f, 0f), 
                    Quaternion.Euler(0, 180, 0)
                ) 
            },
            
            { 
                "Irohazaka_Uphill", 
                new RouteInfo(
                    "Irohazaka Uphill", 
                    "IROHAZAKA",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(0f, 100f, 500f), 
                    new Vector3(-5f, 100f, 500f), 
                    Quaternion.Euler(0, 0, 0)
                ) 
            },

            // ========== Usui ==========
            { 
                "Usui_Downhill", 
                new RouteInfo(
                    "Usui Downhill", 
                    "USUI",
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(0f, 300f, 0f), 
                    new Vector3(-5f, 300f, 0f), 
                    Quaternion.Euler(0, 180, 0)
                ) 
            },
            
            { 
                "Usui_Uphill", 
                new RouteInfo(
                    "Usui Uphill", 
                    "USUI",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(0f, 100f, 400f), 
                    new Vector3(-5f, 100f, 400f), 
                    Quaternion.Euler(0, 0, 0)
                ) 
            },

            // ========== Myogi ==========
            { 
                "Myogi_Downhill", 
                new RouteInfo(
                    "Myogi Downhill", 
                    "MYOGI",
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(0f, 300f, 0f), 
                    new Vector3(-5f, 300f, 0f), 
                    Quaternion.Euler(0, 180, 0)
                ) 
            },
            
            { 
                "Myogi_Uphill", 
                new RouteInfo(
                    "Myogi Uphill", 
                    "MYOGI",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(0f, 100f, 400f), 
                    new Vector3(-5f, 100f, 400f), 
                    Quaternion.Euler(0, 0, 0)
                ) 
            },
        };
    }
}
