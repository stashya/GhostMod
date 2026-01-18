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
                    "Akina", 
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
                    "Akina",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(-1380.21f, -145.24f, -1100.04f), 
                    new Vector3(-1382.58f, -145.24f, -1098.29f), 
                    Quaternion.Euler(0.44f, 31.11f, 0f),
                    new Vector3(875.12f, 136.17f, 1144.51f)  // Custom finish at downhill start
                ) 
            },

            // ========== Akagi ==========
            { 
                "Akagi_Downhill", 
                new RouteInfo(
                    "Akagi Downhill", 
                    "AKAGI",
                    "StartingPointP1_Reverse", 
                    "StartingPointP2_Reverse", 
                    "FinishZone_Reverse",
                    new Vector3(-337.15f, 140.59f, -1047.06f), 
                    new Vector3(-342f, 140.59f, -1047.06f), 
                    Quaternion.Euler(359.53f, 187.47f, 0f)
                ) 
            },
            
            { 
                "Akagi_Uphill", 
                new RouteInfo(
                    "Akagi Uphill", 
                    "AKAGI",
                    "StartingPointP1", 
                    "StartingPointP2", 
                    "FinishZone",
                    new Vector3(686.56f, -133.93f, 331.96f), 
                    new Vector3(681f, -133.93f, 331.96f), 
                    Quaternion.Euler(357.35f, 303.44f, 0f),
                    new Vector3(-337.15f, 140.59f, -1047.06f)  // Custom finish at top
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
                    new Vector3(-1309f, -288.36f, 217.47f), 
                    new Vector3(-1316.79f, -288.80f, 217.47f), 
                    Quaternion.Euler(355.13f, 130f, 0f),
                    new Vector3(-247f, 204f, 555f)  // Custom finish at top of hill
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
                    new Vector3(1366.83f, 66.03f, 784.26f), 
                    new Vector3(1366.64f, 66.07f, 779.14f), 
                    Quaternion.Euler(0.21f, 272.14f, 359.58f)
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
                    new Vector3(-1553.90f, -213.33f, -715.23f), 
                    new Vector3(-1558.73f, -213.40f, -715.97f), 
                    Quaternion.Euler(0.33f, 352.15f, 0f),
                    new Vector3(1366.83f, 66.08f, 784.26f)  // Custom finish at downhill start
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