using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using Photon.Pun;
using GhostMod.Models;
using GhostMod.Data;
using GhostMod.Services;
using GhostMod.Components;
using GhostMod.UI;

namespace GhostMod
{
    /// <summary>
    /// Main ghost racing controller
    /// Manages race state, recording, playback, and coordinates UI/services
    /// </summary>
    public class GhostRacingManager : MonoBehaviour
    {
        #region Singleton
        
        public static GhostRacingManager Instance { get; private set; }
        
        #endregion

        #region Constants
        
        private const int SAMPLE_RATE = 60;
        private const float SAMPLE_INTERVAL = 1f / SAMPLE_RATE;
        private const float MAX_RACE_TIME = 700f;
        
        #endregion

        #region Enums
        
        public enum GhostRaceState
        {
            Idle,
            MenuOpen,
            Countdown,
            Recording,
            Racing,
            Finished,
            Cancelled
        }
        
        #endregion

        #region State
        
        public GhostRaceState CurrentState { get; private set; } = GhostRaceState.Idle;
        
        // Services
        private GhostCarService ghostCarService;
        
        // UI
        private GhostMenu menu;
        private RaceHUD raceHUD;
        private GameObject watermarkLabel;
        
        // Race state
        private RouteInfo currentRoute;
        private GhostData currentGhostData;
        private GhostData recordingData;
        private GhostData personalBestGhost;
        private string selectedRouteKey;
        
        private float raceStartTime;
        private float lastSampleTime;
        private int currentPlaybackFrame;
        private int playerProgressFrame;
        private bool isFirstRun;
        private bool isRacingSharedGhost;
        
        // Cached references
        private RCC_CarControllerV3 playerCar;
        private Transform playerTransform;
        private RaceManager raceManager;
        
        // Camera control (standalone mode)
        private bool cameraWasDisabled;
        private bool warmTofuModDetected;
        
        // WarmTofuMod compatibility
        private static Type warmTofuModType;
        private static FieldInfo externalMenuOpenField;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Initialize services
            ghostCarService = new GhostCarService();
            menu = new GhostMenu(this);
            raceHUD = new RaceHUD(this);
            
            // Ensure folders exist
            GhostFileService.EnsureFoldersExist();
            
            // Detect WarmTofuMod
            DetectWarmTofuMod();
            
            Plugin.Log.LogInfo("GhostRacingManager initialized");
        }
        
        private void Update()
        {
            // Lazy create watermark
            if (watermarkLabel == null)
            {
                CreateWatermark();
            }
            
            // G key to toggle menu
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (CurrentState == GhostRaceState.Idle || CurrentState == GhostRaceState.MenuOpen)
                {
                    if (menu.IsOpen)
                    {
                        CloseMenu();
                    }
                    else
                    {
                        OpenMenu();
                    }
                }
            }
            
            // ESC to close menu
            if (menu.IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
            }
            
            // Handle race states
            switch (CurrentState)
            {
                case GhostRaceState.Recording:
                    UpdateRecording();
                    CheckTimeout();
                    CheckCancellation();
                    break;
                    
                case GhostRaceState.Racing:
                    UpdateRecording();
                    UpdateGhostPlayback();
                    CheckTimeout();
                    CheckCancellation();
                    
                    // H key to toggle ghost visibility
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        ghostCarService.ToggleVisibility();
                    }
                    break;
            }
        }
        
        private void LateUpdate()
        {
            // Ensure camera stays disabled while menu is open (standalone mode)
            if (!warmTofuModDetected && cameraWasDisabled)
            {
                ObscuredPrefs.SetInt("ONTYPING", 10);
            }
        }
        
        private void OnGUI()
        {
            menu.Draw();
            
            if (CurrentState == GhostRaceState.Recording || CurrentState == GhostRaceState.Racing)
            {
                float elapsed = Time.time - raceStartTime;
                raceHUD.Draw(elapsed, isFirstRun, currentGhostData, playerProgressFrame, ghostCarService.IsVisible);
            }
        }
        
        private void OnDisable()
        {
            ReleaseCameraControl();
        }
        
        private void OnDestroy()
        {
            ReleaseCameraControl();
            ghostCarService?.Cleanup();
        }
        
        #endregion

        #region Menu Control
        
        private void OpenMenu()
        {
            menu.Open();
            CurrentState = GhostRaceState.MenuOpen;
            AcquireCameraControl();
        }
        
        public void CloseMenu()
        {
            menu.Close();
            CurrentState = GhostRaceState.Idle;
            ReleaseCameraControl();
        }
        
        #endregion

        #region Camera Control
        
        private void DetectWarmTofuMod()
        {
            try
            {
                // Try to find WarmTofuMod
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    warmTofuModType = assembly.GetType("WarmTofuMod.WarmTofuMod");
                    if (warmTofuModType != null) break;
                }
                
                if (warmTofuModType != null)
                {
                    externalMenuOpenField = warmTofuModType.GetField("externalMenuOpen", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (externalMenuOpenField != null)
                    {
                        warmTofuModDetected = true;
                        Plugin.Log.LogInfo("WarmTofuMod detected - using compatible camera control");
                        return;
                    }
                }
                
                warmTofuModDetected = false;
                Plugin.Log.LogInfo("WarmTofuMod not detected - using standalone camera control");
            }
            catch (Exception e)
            {
                warmTofuModDetected = false;
                Plugin.Log.LogWarning($"Error detecting WarmTofuMod: {e.Message}");
            }
        }
        
        private void AcquireCameraControl()
        {
            if (warmTofuModDetected && externalMenuOpenField != null)
            {
                // Use WarmTofuMod's system
                externalMenuOpenField.SetValue(null, true);
            }
            else
            {
                // Standalone: disable camera via ONTYPING
                ObscuredPrefs.SetInt("ONTYPING", 10);
                cameraWasDisabled = true;
            }
        }
        
        private void ReleaseCameraControl()
        {
            if (warmTofuModDetected && externalMenuOpenField != null)
            {
                externalMenuOpenField.SetValue(null, false);
            }
            else if (cameraWasDisabled)
            {
                ObscuredPrefs.SetInt("ONTYPING", 0);
                cameraWasDisabled = false;
            }
        }
        
        #endregion

        #region Race Logic
        
        public void StartGhostRace(string routeKey)
        {
            if (!Routes.All.ContainsKey(routeKey))
            {
                Plugin.Log.LogError($"Route not found: {routeKey}");
                return;
            }
            
            CloseMenu();
            isRacingSharedGhost = false;
            
            currentRoute = Routes.All[routeKey];
            selectedRouteKey = routeKey;
            
            // Get player car
            playerCar = RCC_SceneManager.Instance?.activePlayerVehicle;
            if (playerCar == null)
            {
                Plugin.Log.LogError("No active player vehicle found!");
                return;
            }
            playerTransform = playerCar.transform;
            
            // Find race manager
            raceManager = FindObjectOfType<RaceManager>();
            
            // Check if ghost exists
            isFirstRun = !GhostFileService.PersonalGhostExists(routeKey);
            
            if (!isFirstRun)
            {
                currentGhostData = GhostFileService.LoadGhost(routeKey);
                if (currentGhostData == null)
                {
                    Plugin.Log.LogWarning("Failed to load ghost, treating as first run");
                    isFirstRun = true;
                }
            }
            
            // Reset tracking
            playerProgressFrame = 0;
            
            // Initialize recording
            recordingData = new GhostData
            {
                RouteName = routeKey,
                CarName = playerCar.gameObject.name,
                RecordedDate = DateTime.Now.Ticks,
                Frames = new List<GhostFrame>()
            };
            
            StartCoroutine(StartCountdown());
        }
        
        public void StartSharedGhostRace(string routeKey, SharedGhostInfo sharedGhost)
        {
            if (!Routes.All.ContainsKey(routeKey))
            {
                Plugin.Log.LogError($"Route not found: {routeKey}");
                return;
            }
            
            CloseMenu();
            isRacingSharedGhost = true;
            
            currentRoute = Routes.All[routeKey];
            selectedRouteKey = routeKey;
            
            // Get player car
            playerCar = RCC_SceneManager.Instance?.activePlayerVehicle;
            if (playerCar == null)
            {
                Plugin.Log.LogError("No active player vehicle found!");
                return;
            }
            playerTransform = playerCar.transform;
            
            // Find race manager
            raceManager = FindObjectOfType<RaceManager>();
            
            // Load shared ghost
            isFirstRun = false;
            currentGhostData = GhostFileService.LoadSharedGhost(sharedGhost);
            if (currentGhostData == null)
            {
                Plugin.Log.LogError("Failed to load shared ghost!");
                isRacingSharedGhost = false;
                return;
            }
            
            // Load personal best for comparison
            personalBestGhost = GhostFileService.LoadGhost(routeKey);
            
            Plugin.Log.LogInfo($"Racing shared ghost: {sharedGhost.PlayerName} ({sharedGhost.TimeString})");
            
            // Reset tracking
            playerProgressFrame = 0;
            
            // Initialize recording
            recordingData = new GhostData
            {
                RouteName = routeKey,
                CarName = playerCar.gameObject.name,
                RecordedDate = DateTime.Now.Ticks,
                Frames = new List<GhostFrame>()
            };
            
            StartCoroutine(StartCountdown());
        }
        
        private IEnumerator StartCountdown()
        {
            CurrentState = GhostRaceState.Countdown;
            
            // Validate references
            if (playerCar == null || currentRoute == null)
            {
                Plugin.Log.LogError("Missing required references for countdown");
                CurrentState = GhostRaceState.Idle;
                yield break;
            }
            
            // Freeze car
            var playerRb = playerCar.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.constraints = RigidbodyConstraints.FreezeAll;
            }
            
            // Teleport to start
            Transform startPoint = FindStartPoint(isFirstRun ? currentRoute.StartPointP1Name : currentRoute.StartPointP2Name);
            if (startPoint != null)
            {
                playerTransform.position = startPoint.position;
                playerTransform.rotation = startPoint.rotation;
            }
            else
            {
                playerTransform.position = isFirstRun ? currentRoute.FallbackStartP1 : currentRoute.FallbackStartP2;
                playerTransform.rotation = currentRoute.FallbackRotation;
            }
            
            // Spawn ghost car if not first run
            if (!isFirstRun && currentGhostData != null)
            {
                ghostCarService.SpawnGhostCar(playerCar);
                
                if (ghostCarService.GhostCarObject != null)
                {
                    Transform ghostStart = FindStartPoint(currentRoute.StartPointP1Name);
                    if (ghostStart != null)
                    {
                        ghostCarService.GhostCarObject.transform.position = ghostStart.position;
                        ghostCarService.GhostCarObject.transform.rotation = ghostStart.rotation;
                    }
                    else
                    {
                        ghostCarService.GhostCarObject.transform.position = currentRoute.FallbackStartP1;
                        ghostCarService.GhostCarObject.transform.rotation = currentRoute.FallbackRotation;
                    }
                }
                else
                {
                    Plugin.Log.LogWarning("Ghost car failed to spawn");
                    isFirstRun = true;
                }
            }
            
            // Set ghost mode for player
            var playerCollider = playerCar.GetComponent<SRPlayerCollider>();
            if (playerCollider != null)
            {
                playerCollider.AppelRPCSetGhostModeV2(10);
            }
            
            // Setup finish zone
            SetupFinishZone();
            
            // Reset car state
            playerCar.speed = 0f;
            playerCar.currentGear = 0;
            
            // Get audio components
            AudioSource audioSource = raceManager?.GetComponent<AudioSource>();
            AudioClip countClick = raceManager?.CountClick;
            
            // Start 3D countdown animation
            if (raceManager?.AnimCompteur != null)
            {
                var animator = raceManager.AnimCompteur.GetComponent<Animator>();
                if (animator != null) animator.Play("3DTime");
            }
            
            // Countdown
            for (int i = 5; i >= 1; i--)
            {
                if (raceManager?.TxtCompteur != null)
                    raceManager.TxtCompteur.text = i.ToString();
                if (audioSource != null && countClick != null)
                    audioSource.PlayOneShot(countClick);
                yield return new WaitForSeconds(1f);
                
                if (playerCar == null)
                {
                    CancelRace("Player car lost");
                    yield break;
                }
            }
            
            // GO!
            if (raceManager?.TxtCompteur != null)
                raceManager.TxtCompteur.text = "GO";
            if (raceManager?.RunningLogo != null)
                raceManager.RunningLogo.SetActive(true);
            
            // Unfreeze car
            if (playerRb != null)
            {
                playerRb.constraints = RigidbodyConstraints.None;
                playerRb.drag = 0.01f;
            }
            
            // Start race
            raceStartTime = Time.time;
            lastSampleTime = 0f;
            currentPlaybackFrame = 0;
            CurrentState = isFirstRun ? GhostRaceState.Recording : GhostRaceState.Racing;
            
            // Clear countdown text
            yield return new WaitForSeconds(1f);
            if (raceManager?.TxtCompteur != null)
                raceManager.TxtCompteur.text = "";
        }
        
        private void UpdateRecording()
        {
            if (playerCar == null) return;
            
            float elapsed = Time.time - raceStartTime;
            
            if (elapsed - lastSampleTime >= SAMPLE_INTERVAL)
            {
                GhostFrame frame = new GhostFrame(elapsed, playerTransform, playerCar);
                recordingData.Frames.Add(frame);
                lastSampleTime = elapsed;
            }
        }
        
        private void UpdateGhostPlayback()
        {
            if (ghostCarService.GhostCarObject == null || currentGhostData == null) return;
            if (currentPlaybackFrame >= currentGhostData.Frames.Count) return;
            
            float elapsed = Time.time - raceStartTime;
            
            // Find frames to interpolate
            while (currentPlaybackFrame < currentGhostData.Frames.Count - 1 &&
                   currentGhostData.Frames[currentPlaybackFrame + 1].Timestamp <= elapsed)
            {
                currentPlaybackFrame++;
            }
            
            if (currentPlaybackFrame >= currentGhostData.Frames.Count - 1)
            {
                var lastFrame = currentGhostData.Frames[currentGhostData.Frames.Count - 1];
                ghostCarService.ApplyFrame(lastFrame);
                ghostCarService.UpdateWheelSpin(lastFrame.Speed, Time.deltaTime);
                return;
            }
            
            // Interpolate
            GhostFrame frameA = currentGhostData.Frames[currentPlaybackFrame];
            GhostFrame frameB = currentGhostData.Frames[currentPlaybackFrame + 1];
            
            float t = (elapsed - frameA.Timestamp) / (frameB.Timestamp - frameA.Timestamp);
            t = Mathf.Clamp01(t);
            
            ghostCarService.ApplyInterpolatedFrame(frameA, frameB, t);
            
            float interpolatedSpeed = Mathf.Lerp(frameA.Speed, frameB.Speed, t);
            ghostCarService.UpdateWheelSpin(interpolatedSpeed, Time.deltaTime);
            
            // Update player progress for delta timer
            UpdatePlayerProgress(playerTransform.position);
        }
        
        private void UpdatePlayerProgress(Vector3 playerPos)
        {
            if (currentGhostData == null || currentGhostData.Frames.Count == 0) return;
            
            int searchEnd = Mathf.Min(currentGhostData.Frames.Count - 1, playerProgressFrame + 300);
            
            float closestDist = float.MaxValue;
            int bestFrame = playerProgressFrame;
            
            for (int i = playerProgressFrame; i <= searchEnd; i++)
            {
                float dist = Vector3.Distance(playerPos, currentGhostData.Frames[i].Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestFrame = i;
                }
            }
            
            if (bestFrame > playerProgressFrame && closestDist < 50f)
            {
                playerProgressFrame = bestFrame;
            }
        }
        
        private void CheckTimeout()
        {
            float elapsed = Time.time - raceStartTime;
            if (elapsed >= MAX_RACE_TIME)
            {
                CancelRace("Time limit exceeded!");
            }
        }
        
        private void CheckCancellation()
        {
            if (PlayerPrefs.GetInt("MenuOpen", 0) == 1)
            {
                CancelRace("Menu opened");
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelRace("Cancelled by player");
            }
        }
        
        public void OnFinishLineCrossed()
        {
            if (CurrentState != GhostRaceState.Recording && CurrentState != GhostRaceState.Racing)
                return;
            
            float finishTime = Time.time - raceStartTime;
            recordingData.TotalTime = finishTime;
            
            Plugin.Log.LogInfo($"Finished! Time: {recordingData.GetTimeString()}");
            
            bool newBest = false;
            bool beatOpponent = false;
            
            if (isRacingSharedGhost)
            {
                // Check if beat opponent
                if (currentGhostData != null && finishTime < currentGhostData.TotalTime)
                {
                    beatOpponent = true;
                }
                
                // Check if beat personal best
                if (personalBestGhost == null || finishTime < personalBestGhost.TotalTime)
                {
                    newBest = true;
                }
            }
            else if (isFirstRun)
            {
                newBest = true;
            }
            else if (currentGhostData != null && finishTime < currentGhostData.TotalTime)
            {
                newBest = true;
                beatOpponent = true;
            }
            
            if (newBest)
            {
                GhostFileService.SaveGhost(selectedRouteKey, recordingData);
            }
            
            CleanupRace();
            CurrentState = GhostRaceState.Finished;
            
            StartCoroutine(ShowFinishMessage(finishTime, newBest, beatOpponent));
        }
        
        private IEnumerator ShowFinishMessage(float time, bool newBest, bool beatOpponent)
        {
            if (raceManager?.RunningLogo != null)
                raceManager.RunningLogo.SetActive(false);
            
            int minutes = (int)(time / 60);
            float seconds = time % 60;
            string timeStr = $"{minutes}:{seconds:00.000}";
            
            if (raceManager?.UIMessage != null)
            {
                var messageText = raceManager.UIMessage.GetComponent<Text>();
                var messageAnim = raceManager.UIMessage.GetComponent<Animator>();
                
                if (messageText != null)
                {
                    if (isRacingSharedGhost)
                    {
                        if (beatOpponent && newBest)
                        {
                            float diff = currentGhostData.TotalTime - time;
                            messageText.text = $"YOU WIN!\n-{diff:0.000}s\nNEW PB SAVED!";
                        }
                        else if (beatOpponent)
                        {
                            float diff = currentGhostData.TotalTime - time;
                            messageText.text = $"YOU WIN!\n-{diff:0.000}s";
                        }
                        else if (newBest)
                        {
                            float diff = time - currentGhostData.TotalTime;
                            messageText.text = $"THEY WIN!\n+{diff:0.000}s\nBut new PB saved!";
                        }
                        else
                        {
                            float diff = time - currentGhostData.TotalTime;
                            messageText.text = $"THEY WIN!\n+{diff:0.000}s";
                        }
                    }
                    else if (newBest)
                    {
                        messageText.text = isFirstRun 
                            ? $"GHOST RECORDED!\nTime: {timeStr}" 
                            : $"NEW RECORD!\nTime: {timeStr}";
                    }
                    else
                    {
                        float diff = time - currentGhostData.TotalTime;
                        messageText.text = $"GHOST WINS!\n+{diff:0.000}s";
                    }
                }
                
                if (messageAnim != null)
                    messageAnim.Play("UIMessage");
            }
            
            isRacingSharedGhost = false;
            personalBestGhost = null;
            
            yield return new WaitForSeconds(4f);
            CurrentState = GhostRaceState.Idle;
        }
        
        private void CancelRace(string reason)
        {
            Plugin.Log.LogInfo($"Race cancelled: {reason}");
            
            if (raceManager?.RunningLogo != null)
                raceManager.RunningLogo.SetActive(false);
            
            if (raceManager?.TxtCompteur != null)
                raceManager.TxtCompteur.text = "";
            
            if (raceManager?.UIMessage != null)
            {
                var messageText = raceManager.UIMessage.GetComponent<Text>();
                var messageAnim = raceManager.UIMessage.GetComponent<Animator>();
                
                if (messageText != null)
                    messageText.text = "GHOST RACE CANCELLED";
                
                if (messageAnim != null)
                    messageAnim.Play("UIMessageShort");
            }
            
            CleanupRace();
            CurrentState = GhostRaceState.Cancelled;
            
            StartCoroutine(ResetToIdle());
        }
        
        private IEnumerator ResetToIdle()
        {
            yield return new WaitForSeconds(2f);
            CurrentState = GhostRaceState.Idle;
        }
        
        private void CleanupRace()
        {
            ghostCarService.Cleanup();
            
            isRacingSharedGhost = false;
            personalBestGhost = null;
            
            if (playerCar != null)
            {
                var playerCollider = playerCar.GetComponent<SRPlayerCollider>();
                if (playerCollider != null)
                {
                    playerCollider.AppelRPCSetGhostModeV2(8);
                }
                
                var rb = playerCar.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.None;
                    rb.drag = 0.01f;
                }
            }
            
            currentPlaybackFrame = 0;
            playerProgressFrame = 0;
        }
        
        #endregion

        #region Helper Methods
        
        private Transform FindStartPoint(string name)
        {
            if (raceManager != null)
            {
                if (name.Contains("P1") && !name.Contains("Reverse"))
                    return raceManager.StartingPointP1;
                if (name.Contains("P2") && !name.Contains("Reverse"))
                    return raceManager.StartingPointP2;
            }
            
            GameObject obj = GameObject.Find(name);
            if (obj != null) return obj.transform;
            
            foreach (var transform in FindObjectsOfType<Transform>())
            {
                if (transform.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return transform;
            }
            
            return null;
        }
        
        private void SetupFinishZone()
        {
            FinishZone[] finishZones = FindObjectsOfType<FinishZone>();
            
            foreach (var zone in finishZones)
            {
                GhostFinishTrigger trigger = zone.gameObject.GetComponent<GhostFinishTrigger>();
                if (trigger == null)
                {
                    trigger = zone.gameObject.AddComponent<GhostFinishTrigger>();
                }
                trigger.Manager = this;
            }
        }
        
        private void CreateWatermark()
        {
            try
            {
                GameObject uiMessage = GameObject.Find("UIMessage");
                if (uiMessage == null) return;
                
                watermarkLabel = Instantiate(uiMessage);
                watermarkLabel.name = "GhostModWatermark";
                watermarkLabel.transform.SetParent(uiMessage.transform.parent);
                
                var msgComponent = watermarkLabel.GetComponent<SRMessageOther>();
                if (msgComponent != null) Destroy(msgComponent);
                var animComponent = watermarkLabel.GetComponent<Animator>();
                if (animComponent != null) Destroy(animComponent);
                
                RectTransform r = watermarkLabel.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(1, 0);
                r.anchorMax = new Vector2(1, 0);
                r.pivot = new Vector2(1, 0);
                r.anchoredPosition = new Vector2(-10, 5);
                r.sizeDelta = new Vector2(300f, 40f);
                
                Text t = watermarkLabel.GetComponent<Text>();
                t.text = $"GhostMod v{PluginInfo.PLUGIN_VERSION}";
                t.alignment = TextAnchor.LowerRight;
                t.transform.localScale = Vector3.one;
                t.resizeTextMaxSize = 18;
                t.color = Color.gray;
                
                watermarkLabel.SetActive(true);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Failed to create watermark: {e.Message}");
            }
        }
        
        #endregion
    }
}
