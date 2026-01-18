using UnityEngine;
using ZionBandwidthOptimizer.Examples;

namespace GhostMod.Components
{
    /// <summary>
    /// Component to detect when player crosses finish line during ghost race
    /// Attaches to existing FinishZone objects in the game
    /// </summary>
    public class GhostFinishTrigger : MonoBehaviour
    {
        /// <summary>
        /// Reference to the ghost racing manager
        /// </summary>
        public GhostRacingManager Manager { get; set; }
        
        private void OnTriggerEnter(Collider other)
        {
            if (Manager == null)
            {
                return;
            }
            
            // Only trigger during active race states
            if (Manager.CurrentState != GhostRacingManager.GhostRaceState.Recording &&
                Manager.CurrentState != GhostRacingManager.GhostRaceState.Racing)
            {
                return;
            }
            
            // Check if it's the player's car
            var photonNetwork = other.GetComponentInParent<RCC_PhotonNetwork>();
            if (photonNetwork != null && photonNetwork.isMine)
            {
                Manager.OnFinishLineCrossed();
            }
        }
    }
}