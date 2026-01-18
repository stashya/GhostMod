using UnityEngine;
using GhostMod.Models;

namespace GhostMod.UI
{
    /// <summary>
    /// In-race HUD showing timer, delta time, and hints
    /// </summary>
    public class RaceHUD
    {
        #region State
        
        private Texture2D bgTexture;
        private GhostRacingManager manager;
        
        #endregion

        #region Constructor
        
        public RaceHUD(GhostRacingManager manager)
        {
            this.manager = manager;
        }
        
        #endregion

        #region Public Methods
        
        public void Draw(float elapsed, bool isFirstRun, GhostData ghostData, int playerProgressFrame, bool ghostVisible)
        {
            EnsureTexture();
            
            int minutes = (int)(elapsed / 60);
            float seconds = elapsed % 60;
            
            // Background style
            GUIStyle boxStyle = new GUIStyle();
            boxStyle.normal.background = bgTexture;
            
            // Calculate box dimensions
            float boxWidth = 200;
            float boxHeight = isFirstRun ? 70 : 105;
            float boxX = Screen.width - boxWidth - 20;
            float boxY = 35;
            
            // Draw background
            GUI.Box(new Rect(boxX - 5, boxY - 5, boxWidth + 10, boxHeight + 10), "", boxStyle);
            
            // Time display
            DrawTime(boxX, boxY, boxWidth, minutes, seconds);
            
            // Mode indicator
            DrawModeIndicator(boxX, boxY, boxWidth, isFirstRun);
            
            // Delta time (if racing against ghost)
            if (!isFirstRun && ghostData != null && ghostData.Frames.Count > 0)
            {
                DrawDeltaTime(boxX, boxY, boxWidth, elapsed, ghostData, playerProgressFrame);
            }
            
            // Bottom hints
            DrawHints(isFirstRun, ghostVisible);
        }
        
        #endregion

        #region Private Methods
        
        private void EnsureTexture()
        {
            if (bgTexture == null)
            {
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.85f));
                bgTexture.Apply();
            }
        }
        
        private void DrawTime(float boxX, float boxY, float boxWidth, int minutes, float seconds)
        {
            GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
            timeStyle.fontSize = 26;
            timeStyle.fontStyle = FontStyle.Bold;
            timeStyle.alignment = TextAnchor.MiddleCenter;
            timeStyle.normal.textColor = Color.white;
            
            string timeText = $"{minutes}:{seconds:00.000}";
            GUI.Label(new Rect(boxX, boxY, boxWidth, 32), timeText, timeStyle);
        }
        
        private void DrawModeIndicator(float boxX, float boxY, float boxWidth, bool isFirstRun)
        {
            GUIStyle modeStyle = new GUIStyle(GUI.skin.label);
            modeStyle.fontSize = 12;
            modeStyle.alignment = TextAnchor.MiddleCenter;
            modeStyle.normal.textColor = isFirstRun 
                ? new Color(1f, 0.85f, 0.2f) 
                : new Color(0.4f, 0.85f, 1f);
            
            string modeText = isFirstRun ? "● RECORDING" : "● RACING GHOST";
            GUI.Label(new Rect(boxX, boxY + 32, boxWidth, 20), modeText, modeStyle);
        }
        
        private void DrawDeltaTime(float boxX, float boxY, float boxWidth, float elapsed, GhostData ghostData, int progressFrame)
        {
            if (progressFrame >= ghostData.Frames.Count)
                progressFrame = ghostData.Frames.Count - 1;
            
            float ghostTimeAtProgress = ghostData.Frames[progressFrame].Timestamp;
            float diff = elapsed - ghostTimeAtProgress;
            
            GUIStyle diffStyle = new GUIStyle(GUI.skin.label);
            diffStyle.fontSize = 20;
            diffStyle.fontStyle = FontStyle.Bold;
            diffStyle.alignment = TextAnchor.MiddleCenter;
            
            string diffText;
            if (diff > 0.01f)
            {
                diffStyle.normal.textColor = new Color(1f, 0.35f, 0.35f); // Red - behind
                diffText = $"+{diff:0.000}";
            }
            else if (diff < -0.01f)
            {
                diffStyle.normal.textColor = new Color(0.35f, 1f, 0.35f); // Green - ahead
                diffText = $"{diff:0.000}";
            }
            else
            {
                diffStyle.normal.textColor = new Color(1f, 1f, 0.35f); // Yellow - even
                diffText = $"{diff:0.000}";
            }
            
            GUI.Label(new Rect(boxX, boxY + 58, boxWidth, 28), diffText, diffStyle);
        }
        
        private void DrawHints(bool isFirstRun, bool ghostVisible)
        {
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 11;
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            
            string hintText = "[ESC] cancel   [M] restart";
            if (!isFirstRun)
            {
                hintText += ghostVisible ? "   [H] hide" : "   [H] show";
            }
            
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 35, 300, 20), hintText, hintStyle);
        }
        
        #endregion
    }
}
