using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using GhostMod.Models;

namespace GhostMod.Services
{
    /// <summary>
    /// Handles ghost car spawning, visual updates, and transparency
    /// </summary>
    public class GhostCarService
    {
        #region Constants
        
        private const float GHOST_TRANSPARENCY = 0.35f;
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        
        #endregion

        #region State
        
        public GameObject GhostCarObject { get; private set; }
        public bool IsVisible { get; private set; } = true;
        
        private List<Renderer> ghostRenderers = new List<Renderer>();
        private List<Material> originalMaterials = new List<Material>();
        private List<Transform> wheelTransforms = new List<Transform>();
        private List<Transform> frontWheelTransforms = new List<Transform>();
        private List<Light> brakeLights = new List<Light>();
        private List<Light> headLights = new List<Light>();
        
        private float wheelRadius = 0.35f;
        private float wheelRotation = 0f;
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Spawn a ghost car based on the player's car
        /// </summary>
        public void SpawnGhostCar(RCC_CarControllerV3 playerCar)
        {
            if (playerCar == null) return;
            
            Cleanup();
            
            // Get wheel paths before creating ghost
            List<string> wheelPaths = new List<string>();
            List<string> frontWheelPaths = new List<string>();
            List<string> brakeLightPaths = new List<string>();
            List<string> headLightPaths = new List<string>();
            
            CollectWheelPaths(playerCar, wheelPaths, frontWheelPaths);
            CollectLightPaths(playerCar, brakeLightPaths, headLightPaths);
            
            // Create ghost car
            GhostCarObject = new GameObject("GhostCar");
            GhostCarObject.transform.position = playerCar.transform.position;
            GhostCarObject.transform.rotation = playerCar.transform.rotation;
            
            // Copy visual hierarchy
            CopyVisualHierarchy(playerCar.transform, GhostCarObject.transform);
            
            // Find components in ghost
            FindWheelsInGhost(wheelPaths, frontWheelPaths);
            FindLightsInGhost(brakeLightPaths, headLightPaths);
            
            // Make transparent
            MakeGhostTransparent();
            
            IsVisible = true;
            Plugin.Log.LogInfo($"Ghost car spawned - Wheels: {wheelTransforms.Count}, Front: {frontWheelTransforms.Count}, Brakes: {brakeLights.Count}");
        }
        
        /// <summary>
        /// Destroy the ghost car and clean up
        /// </summary>
        public void Cleanup()
        {
            if (GhostCarObject != null)
            {
                UnityEngine.Object.Destroy(GhostCarObject);
                GhostCarObject = null;
            }
            
            ghostRenderers.Clear();
            originalMaterials.Clear();
            wheelTransforms.Clear();
            frontWheelTransforms.Clear();
            brakeLights.Clear();
            headLights.Clear();
            wheelRotation = 0f;
        }
        
        /// <summary>
        /// Toggle ghost visibility
        /// </summary>
        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
            if (GhostCarObject != null)
            {
                GhostCarObject.SetActive(IsVisible);
            }
        }
        
        /// <summary>
        /// Set ghost visibility
        /// </summary>
        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            if (GhostCarObject != null)
            {
                GhostCarObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Apply a ghost frame to the ghost car
        /// </summary>
        public void ApplyFrame(GhostFrame frame)
        {
            if (GhostCarObject == null) return;
            
            try
            {
                GhostCarObject.transform.position = frame.Position;
                GhostCarObject.transform.rotation = frame.Rotation;
                ApplyWheelSteering(frame.WheelSteerAngle);
                ApplyLights(frame.IsBraking, frame.HasHeadlights);
            }
            catch
            {
                // Ignore errors - ghost is purely visual
            }
        }
        
        /// <summary>
        /// Apply interpolated frame between two ghost frames
        /// </summary>
        public void ApplyInterpolatedFrame(GhostFrame a, GhostFrame b, float t)
        {
            if (GhostCarObject == null) return;
            
            try
            {
                GhostCarObject.transform.position = Vector3.Lerp(a.Position, b.Position, t);
                GhostCarObject.transform.rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
                
                float wheelAngle = Mathf.Lerp(a.WheelSteerAngle, b.WheelSteerAngle, t);
                ApplyWheelSteering(wheelAngle);
                
                bool braking = t < 0.5f ? a.IsBraking : b.IsBraking;
                bool headlights = t < 0.5f ? a.HasHeadlights : b.HasHeadlights;
                ApplyLights(braking, headlights);
            }
            catch
            {
                // Ignore errors - ghost is purely visual
            }
        }
        
        /// <summary>
        /// Update wheel spin based on speed
        /// </summary>
        public void UpdateWheelSpin(float speed, float deltaTime)
        {
            float circumference = 2f * Mathf.PI * wheelRadius;
            float rotationDelta = (speed / circumference) * 360f * deltaTime;
            wheelRotation += rotationDelta;
            
            // Keep in reasonable range
            if (wheelRotation > 3600f) wheelRotation -= 3600f;
            if (wheelRotation < -3600f) wheelRotation += 3600f;
            
            // Apply to non-front wheels
            foreach (var wheel in wheelTransforms)
            {
                if (wheel == null || frontWheelTransforms.Contains(wheel)) continue;
                
                try
                {
                    Vector3 euler = wheel.localEulerAngles;
                    wheel.localEulerAngles = new Vector3(wheelRotation, euler.y, euler.z);
                }
                catch { }
            }
        }
        
        #endregion

        #region Private Methods
        
        private void CollectWheelPaths(RCC_CarControllerV3 car, List<string> wheelPaths, List<string> frontWheelPaths)
        {
            try
            {
                var wheelColliders = car.GetComponentsInChildren<RCC_WheelCollider>(true);
                foreach (var wc in wheelColliders)
                {
                    var wheelModelField = typeof(RCC_WheelCollider).GetField("wheelModel", BINDING_FLAGS);
                    if (wheelModelField != null)
                    {
                        Transform wheelModel = wheelModelField.GetValue(wc) as Transform;
                        if (wheelModel != null)
                        {
                            string path = GetTransformPath(wheelModel, car.transform);
                            wheelPaths.Add(path);
                            
                            string wcName = wc.gameObject.name.ToLower();
                            if (wcName.Contains("front") || wcName.Contains("fl") || wcName.Contains("fr"))
                            {
                                frontWheelPaths.Add(path);
                            }
                        }
                    }
                    
                    if (wc.wheelCollider != null)
                    {
                        wheelRadius = wc.wheelCollider.radius;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Error getting wheel paths: {e.Message}");
            }
        }
        
        private void CollectLightPaths(RCC_CarControllerV3 car, List<string> brakeLightPaths, List<string> headLightPaths)
        {
            try
            {
                var rccLights = car.GetComponentsInChildren<RCC_Light>(true);
                foreach (var rccLight in rccLights)
                {
                    Light unityLight = rccLight.GetComponent<Light>();
                    if (unityLight != null)
                    {
                        string path = GetTransformPath(rccLight.transform, car.transform);
                        if (rccLight.lightType == RCC_Light.LightType.BrakeLight)
                        {
                            brakeLightPaths.Add(path);
                        }
                        else if (rccLight.lightType == RCC_Light.LightType.HeadLight || 
                                 rccLight.lightType == RCC_Light.LightType.HighBeamHeadLight)
                        {
                            headLightPaths.Add(path);
                        }
                    }
                }
            }
            catch { }
        }
        
        private void FindWheelsInGhost(List<string> wheelPaths, List<string> frontWheelPaths)
        {
            foreach (string path in wheelPaths)
            {
                Transform wheel = GhostCarObject.transform.Find(path);
                if (wheel != null)
                {
                    wheelTransforms.Add(wheel);
                    if (frontWheelPaths.Contains(path))
                    {
                        frontWheelTransforms.Add(wheel);
                    }
                }
            }
        }
        
        private void FindLightsInGhost(List<string> brakeLightPaths, List<string> headLightPaths)
        {
            foreach (string path in brakeLightPaths)
            {
                Transform lightT = GhostCarObject.transform.Find(path);
                if (lightT != null)
                {
                    Light light = lightT.GetComponent<Light>();
                    if (light != null)
                    {
                        brakeLights.Add(light);
                        light.intensity = 0f;
                    }
                }
            }
            
            foreach (string path in headLightPaths)
            {
                Transform lightT = GhostCarObject.transform.Find(path);
                if (lightT != null)
                {
                    Light light = lightT.GetComponent<Light>();
                    if (light != null)
                    {
                        headLights.Add(light);
                        light.intensity = 0f;
                    }
                }
            }
        }
        
        private string GetTransformPath(Transform target, Transform root)
        {
            if (target == root) return "";
            
            List<string> pathParts = new List<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                pathParts.Insert(0, current.name);
                current = current.parent;
            }
            return string.Join("/", pathParts);
        }
        
        private void CopyVisualHierarchy(Transform source, Transform dest)
        {
            string nameLower = source.name.ToLower();
            
            // Skip non-visual objects
            if (nameLower.Contains("collider") || 
                nameLower.Contains("trigger") ||
                nameLower.Contains("sensor") ||
                nameLower.Contains("raycast") ||
                nameLower.Contains("audio") ||
                nameLower.Contains("sound") ||
                nameLower.Contains("particle") ||
                nameLower.Contains("effect") ||
                nameLower.Contains("spawn") ||
                nameLower.Contains("navmesh") ||
                nameLower.Contains("waypoint"))
            {
                return;
            }
            
            // Copy MeshFilter + MeshRenderer
            MeshFilter sourceMF = source.GetComponent<MeshFilter>();
            MeshRenderer sourceMR = source.GetComponent<MeshRenderer>();
            if (sourceMF != null && sourceMR != null && sourceMR.enabled && sourceMF.sharedMesh != null)
            {
                string meshName = sourceMF.sharedMesh.name.ToLower();
                if (!meshName.Contains("cube") && 
                    !meshName.Contains("debug") &&
                    !meshName.Contains("bounds") &&
                    !meshName.Contains("collision"))
                {
                    MeshFilter destMF = dest.gameObject.AddComponent<MeshFilter>();
                    destMF.sharedMesh = sourceMF.sharedMesh;
                    
                    MeshRenderer destMR = dest.gameObject.AddComponent<MeshRenderer>();
                    destMR.sharedMaterials = sourceMR.sharedMaterials;
                    destMR.shadowCastingMode = sourceMR.shadowCastingMode;
                }
            }
            
            // Copy SkinnedMeshRenderer
            SkinnedMeshRenderer sourceSMR = source.GetComponent<SkinnedMeshRenderer>();
            if (sourceSMR != null && sourceSMR.enabled && sourceSMR.sharedMesh != null)
            {
                SkinnedMeshRenderer destSMR = dest.gameObject.AddComponent<SkinnedMeshRenderer>();
                destSMR.sharedMesh = sourceSMR.sharedMesh;
                destSMR.sharedMaterials = sourceSMR.sharedMaterials;
            }
            
            // Copy Light
            Light sourceLight = source.GetComponent<Light>();
            if (sourceLight != null && sourceLight.enabled)
            {
                Light destLight = dest.gameObject.AddComponent<Light>();
                destLight.type = sourceLight.type;
                destLight.color = sourceLight.color;
                destLight.intensity = sourceLight.intensity;
                destLight.range = sourceLight.range;
                destLight.spotAngle = sourceLight.spotAngle;
            }
            
            // Recursively copy children
            foreach (Transform child in source)
            {
                GameObject childCopy = new GameObject(child.name);
                childCopy.transform.SetParent(dest);
                childCopy.transform.localPosition = child.localPosition;
                childCopy.transform.localRotation = child.localRotation;
                childCopy.transform.localScale = child.localScale;
                
                CopyVisualHierarchy(child, childCopy.transform);
            }
        }
        
        private void MakeGhostTransparent()
        {
            ghostRenderers.Clear();
            originalMaterials.Clear();
            
            foreach (var renderer in GhostCarObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    ghostRenderers.Add(renderer);
                    
                    Material[] newMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        Material originalMat = renderer.materials[i];
                        originalMaterials.Add(originalMat);
                        
                        Material ghostMat = new Material(Shader.Find("Standard"));
                        
                        if (originalMat.HasProperty("_MainTex") && originalMat.GetTexture("_MainTex") != null)
                            ghostMat.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
                        
                        Color baseColor = Color.white;
                        if (originalMat.HasProperty("_Color"))
                            baseColor = originalMat.GetColor("_Color");
                        
                        Color ghostTint = new Color(0.7f, 0.9f, 1.0f, 1f);
                        ghostMat.SetColor("_Color", Color.Lerp(baseColor, ghostTint, 1f - GHOST_TRANSPARENCY));
                        
                        ghostMat.EnableKeyword("_EMISSION");
                        ghostMat.SetColor("_EmissionColor", new Color(0.15f, 0.25f, 0.35f, 1f));
                        
                        ghostMat.SetFloat("_Mode", 0);
                        ghostMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        ghostMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        ghostMat.SetInt("_ZWrite", 1);
                        ghostMat.renderQueue = -1;
                        
                        newMaterials[i] = ghostMat;
                    }
                    renderer.materials = newMaterials;
                }
                catch { }
            }
        }
        
        private void ApplyWheelSteering(float steerAngle)
        {
            foreach (var wheel in frontWheelTransforms)
            {
                if (wheel == null) continue;
                
                try
                {
                    wheel.localEulerAngles = new Vector3(wheelRotation, steerAngle, 0f);
                }
                catch { }
            }
        }
        
        private void ApplyLights(bool braking, bool headlights)
        {
            foreach (var light in brakeLights)
            {
                if (light != null)
                    light.intensity = braking ? 2f : 0f;
            }
            
            foreach (var light in headLights)
            {
                if (light != null)
                    light.intensity = headlights ? 1f : 0f;
            }
        }
        
        #endregion
    }
}
