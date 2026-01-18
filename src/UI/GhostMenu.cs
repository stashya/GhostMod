using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GhostMod.Data;
using GhostMod.Models;
using GhostMod.Services;

namespace GhostMod.UI
{
    /// <summary>
    /// Main ghost racing menu GUI
    /// </summary>
    public class GhostMenu
    {
        #region State
        
        public bool IsOpen { get; private set; }
        
        private Rect windowRect = new Rect(100, 100, 340, 420);
        private Vector2 menuScrollPosition;
        private Vector2 sharedGhostScroll;
        
        private string selectedRouteKey;
        private string menuMessage = "";
        private Dictionary<string, string> ghostTimeCache = new Dictionary<string, string>();
        
        private List<SharedGhostInfo> sharedGhosts = new List<SharedGhostInfo>();
        private bool showSharedGhostList;
        private SharedGhostInfo selectedSharedGhost;
        
        private GhostRacingManager manager;
        
        #endregion

        #region Constructor
        
        public GhostMenu(GhostRacingManager manager)
        {
            this.manager = manager;
        }
        
        #endregion

        #region Public Methods
        
        public void Open()
        {
            IsOpen = true;
            RefreshCache();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void Close()
        {
            IsOpen = false;
            showSharedGhostList = false;
            selectedSharedGhost = null;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        public void Draw()
        {
            if (!IsOpen) return;
            windowRect = GUI.Window(9999, windowRect, DrawWindow, $"GHOST RACING v{PluginInfo.PLUGIN_VERSION} [G]");
        }
        
        public void RefreshCache()
        {
            ghostTimeCache.Clear();
            sharedGhosts = GhostFileService.ScanSharedGhosts();
        }
        
        public void SetMessage(string message)
        {
            menuMessage = message;
        }
        
        #endregion

        #region Private Methods
        
        private void DrawWindow(int windowID)
        {
            menuScrollPosition = GUILayout.BeginScrollView(menuScrollPosition);
            
            GUILayout.Space(5);
            
            // Current map
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
            GUILayout.Box("MAP: " + currentScene.ToUpper(), GUILayout.Height(28));
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            // Get available routes
            var availableRoutes = Routes.All.Where(r => 
                r.Value.SceneName.Equals(currentScene, System.StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (availableRoutes.Count == 0)
            {
                DrawNoRoutesMessage();
            }
            else
            {
                DrawRouteSelection(availableRoutes);
                DrawActionButtons();
                DrawSharedGhostsSection();
            }
            
            GUILayout.Space(8);
            
            // Status message
            if (!string.IsNullOrEmpty(menuMessage))
            {
                GUILayout.Label("<color=lime>" + menuMessage + "</color>");
            }
            
            GUILayout.EndScrollView();
            
            // Close button
            GUILayout.Space(5);
            if (GUILayout.Button("Close", GUILayout.Height(28)))
            {
                manager.CloseMenu();
            }
            
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 25));
        }
        
        private void DrawNoRoutesMessage()
        {
            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            GUILayout.Box("NO ROUTES ON THIS MAP!", GUILayout.Height(26));
            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);
            GUILayout.Label("Available maps:");
            GUILayout.Label("• Akina, Akagi, Irohazaka");
            GUILayout.Label("• Usui, Myogi");
        }
        
        private void DrawRouteSelection(List<KeyValuePair<string, RouteInfo>> routes)
        {
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            GUILayout.Box("=== SELECT ROUTE ===", GUILayout.Height(26));
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(3);
            
            foreach (var route in routes)
            {
                bool isSelected = selectedRouteKey == route.Key;
                bool hasGhost = GhostFileService.PersonalGhostExists(route.Key);
                
                // Get cached time
                string timeStr = "";
                if (hasGhost)
                {
                    if (!ghostTimeCache.ContainsKey(route.Key))
                    {
                        GhostData ghost = GhostFileService.LoadGhost(route.Key);
                        ghostTimeCache[route.Key] = ghost != null ? ghost.GetTimeString() : "???";
                    }
                    timeStr = ghostTimeCache[route.Key];
                }
                
                // Button colors
                if (isSelected)
                    GUI.backgroundColor = Color.green;
                else if (hasGhost)
                    GUI.backgroundColor = new Color(0.7f, 0.5f, 1f);
                else
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);
                
                string btnText = hasGhost 
                    ? $"{route.Value.RouteName}  [{timeStr}]" 
                    : route.Value.RouteName;
                
                if (GUILayout.Button(btnText, GUILayout.Height(32)))
                {
                    selectedRouteKey = route.Key;
                }
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        private void DrawActionButtons()
        {
            if (selectedRouteKey == null) return;
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
            GUILayout.Box("=== ACTION ===", GUILayout.Height(26));
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            bool ghostExists = GhostFileService.PersonalGhostExists(selectedRouteKey);
            
            // Start race button
            GUI.backgroundColor = ghostExists ? Color.cyan : Color.green;
            string startText = ghostExists ? "RACE YOUR GHOST!" : "SET INITIAL TIME";
            if (GUILayout.Button(startText, GUILayout.Height(45)))
            {
                manager.StartGhostRace(selectedRouteKey);
            }
            GUI.backgroundColor = Color.white;
            
            // Delete ghost button
            if (ghostExists)
            {
                GUILayout.Space(5);
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete Ghost", GUILayout.Height(28)))
                {
                    GhostFileService.DeleteGhost(selectedRouteKey);
                    menuMessage = "Ghost deleted!";
                    ghostTimeCache.Remove(selectedRouteKey);
                }
                GUI.backgroundColor = Color.white;
            }
        }
        
        private void DrawSharedGhostsSection()
        {
            if (selectedRouteKey == null) return;
            
            GUILayout.Space(10);
            
            var routeSharedGhosts = sharedGhosts
                .Where(g => g.RouteKey == selectedRouteKey)
                .OrderBy(g => g.TotalTime)
                .ToList();
            
            if (routeSharedGhosts.Count > 0)
            {
                GUI.backgroundColor = new Color(1f, 0.5f, 0f);
                GUILayout.Box("=== CHALLENGE OTHERS ===", GUILayout.Height(26));
                GUI.backgroundColor = Color.white;
                
                GUILayout.Space(3);
                
                // Toggle list
                GUI.backgroundColor = showSharedGhostList ? Color.yellow : new Color(0.8f, 0.6f, 0.2f);
                if (GUILayout.Button(showSharedGhostList ? "Hide Shared Ghosts" : $"Show Shared Ghosts ({routeSharedGhosts.Count})", GUILayout.Height(28)))
                {
                    showSharedGhostList = !showSharedGhostList;
                    if (!showSharedGhostList)
                        selectedSharedGhost = null;
                }
                GUI.backgroundColor = Color.white;
                
                // Show list
                if (showSharedGhostList)
                {
                    GUILayout.Space(3);
                    sharedGhostScroll = GUILayout.BeginScrollView(sharedGhostScroll, 
                        GUILayout.Height(Mathf.Min(routeSharedGhosts.Count * 34 + 10, 150)));
                    
                    foreach (var shared in routeSharedGhosts)
                    {
                        bool isSelected = selectedSharedGhost == shared;
                        GUI.backgroundColor = isSelected ? Color.yellow : new Color(0.9f, 0.7f, 0.4f);
                        
                        string btnText = $"{shared.PlayerName} - {shared.TimeString}";
                        if (GUILayout.Button(btnText, GUILayout.Height(30)))
                        {
                            selectedSharedGhost = shared;
                        }
                    }
                    
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndScrollView();
                    
                    // Race selected ghost
                    if (selectedSharedGhost != null)
                    {
                        GUILayout.Space(5);
                        GUI.backgroundColor = new Color(1f, 0.6f, 0f);
                        if (GUILayout.Button($"RACE {selectedSharedGhost.PlayerName}!", GUILayout.Height(40)))
                        {
                            manager.StartSharedGhostRace(selectedRouteKey, selectedSharedGhost);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
            else
            {
                GUILayout.Space(5);
                GUILayout.Label("<color=#888888>Drop .ghost files in /ghosts/shared/</color>");
                GUILayout.Label("<color=#888888>to race other players!</color>");
            }
        }
        
        #endregion
    }
}
