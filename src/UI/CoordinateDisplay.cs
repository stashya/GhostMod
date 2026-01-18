using UnityEngine;

namespace GhostMod.UI
{
    /// <summary>
    /// Displays current player coordinates on screen
    /// Press C to toggle visibility
    /// Use this to find coordinates for setting up new routes
    /// </summary>
    public class CoordinateDisplay : MonoBehaviour
    {
        #region State
        
        private bool isVisible = false;
        private RCC_CarControllerV3 playerCar;
        
        // Cached GUI styles
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private GUIStyle headerStyle;
        private GUIStyle valueStyle;
        private bool stylesInitialized = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            // Toggle with N key
            if (Input.GetKeyDown(KeyCode.N))
            {
                isVisible = !isVisible;
            }
            
            // Update player car reference
            if (isVisible && (playerCar == null || !playerCar.gameObject.activeInHierarchy))
            {
                playerCar = RCC_SceneManager.Instance?.activePlayerVehicle;
            }
        }
        
        private void OnGUI()
        {
            if (!isVisible) return;
            
            // Initialize styles once
            if (!stylesInitialized)
            {
                InitializeStyles();
            }
            
            // Position: middle-right of screen
            float boxWidth = 220f;
            float boxHeight = 220f;
            float xPos = Screen.width - boxWidth - 20f;
            float yPos = (Screen.height - boxHeight) / 2f;
            
            Rect boxRect = new Rect(xPos, yPos, boxWidth, boxHeight);
            
            // Draw background box
            GUI.Box(boxRect, "", boxStyle);
            
            // Content area
            GUILayout.BeginArea(new Rect(xPos + 10f, yPos + 10f, boxWidth - 20f, boxHeight - 20f));
            
            // Header
            GUILayout.Label("POSITION", headerStyle);
            GUILayout.Space(5f);
            
            if (playerCar != null)
            {
                Vector3 pos = playerCar.transform.position;
                Vector3 rot = playerCar.transform.eulerAngles;
                
                // Position values
                DrawCoordRow("X:", pos.x);
                DrawCoordRow("Y:", pos.y);
                DrawCoordRow("Z:", pos.z);
                
                GUILayout.Space(8f);
                
                // Rotation values (all 3 axes)
                GUILayout.Label("ROTATION", headerStyle);
                GUILayout.Space(3f);
                DrawCoordRow("X (Pitch):", rot.x, "°");
                DrawCoordRow("Y (Yaw):", rot.y, "°");
                DrawCoordRow("Z (Roll):", rot.z, "°");
                
                GUILayout.Space(8f);
                
                // Hint
                GUILayout.Label("[N] Toggle", labelStyle);
            }
            else
            {
                GUILayout.Label("No vehicle found", labelStyle);
                GUILayout.Space(10f);
                GUILayout.Label("[N] Toggle", labelStyle);
            }
            
            GUILayout.EndArea();
        }
        
        #endregion

        #region Private Methods
        
        private void InitializeStyles()
        {
            // Background box
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0f, 0f, 0f, 0.85f));
            
            // Regular label
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.gray;
            labelStyle.fontSize = 14;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            
            // Header
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.normal.textColor = Color.cyan;
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            
            // Value display
            valueStyle = new GUIStyle(GUI.skin.label);
            valueStyle.normal.textColor = Color.white;
            valueStyle.fontSize = 14;
            valueStyle.alignment = TextAnchor.MiddleRight;
            
            stylesInitialized = true;
        }
        
        private void DrawCoordRow(string label, float value, string suffix = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(75f));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{value:F2}{suffix}", valueStyle);
            GUILayout.EndHorizontal();
        }
        
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        #endregion
    }
}