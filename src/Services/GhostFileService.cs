using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GhostMod.Models;
using GhostMod.Data;

namespace GhostMod.Services
{
    /// <summary>
    /// Handles all ghost file operations with security validation
    /// </summary>
    public static class GhostFileService
    {
        #region Constants
        
        private const string GHOST_FOLDER = "BepInEx/plugins/GhostMod/ghosts";
        private const string PERSONAL_FOLDER = "personal";
        private const string SHARED_FOLDER = "shared";
        private const string FILE_MAGIC = "GHOST";
        private const int FILE_VERSION = 1;
        
        // Security limits
        private const long MAX_GHOST_FILE_SIZE = 50 * 1024 * 1024; // 50MB max
        private const int MAX_FRAME_COUNT = 100000; // ~27 minutes at 60fps
        private const int MAX_STRING_LENGTH = 256;
        private const float MAX_VALID_TIME = 1800f; // 30 minutes max
        private const float MIN_VALID_TIME = 5f; // 5 seconds min
        private const int MAX_SHARED_GHOST_FILES = 100;
        
        #endregion

        #region Folder Management
        
        /// <summary>
        /// Get the base ghosts folder path
        /// </summary>
        public static string GetBasePath()
        {
            return Path.Combine(Application.dataPath, "..", GHOST_FOLDER);
        }
        
        /// <summary>
        /// Get the personal ghosts folder path
        /// </summary>
        public static string GetPersonalPath()
        {
            return Path.Combine(GetBasePath(), PERSONAL_FOLDER);
        }
        
        /// <summary>
        /// Get the shared ghosts folder path
        /// </summary>
        public static string GetSharedPath()
        {
            return Path.Combine(GetBasePath(), SHARED_FOLDER);
        }
        
        /// <summary>
        /// Ensure all ghost folders exist
        /// </summary>
        public static void EnsureFoldersExist()
        {
            string personalPath = GetPersonalPath();
            string sharedPath = GetSharedPath();
            
            if (!Directory.Exists(personalPath))
            {
                Directory.CreateDirectory(personalPath);
                Plugin.Log.LogInfo($"Created personal ghosts folder: {personalPath}");
            }
            
            if (!Directory.Exists(sharedPath))
            {
                Directory.CreateDirectory(sharedPath);
                Plugin.Log.LogInfo($"Created shared ghosts folder: {sharedPath}");
            }
        }
        
        /// <summary>
        /// Get the file path for a personal ghost
        /// </summary>
        public static string GetPersonalGhostPath(string routeKey)
        {
            return Path.Combine(GetPersonalPath(), $"{routeKey}.ghost");
        }
        
        #endregion

        #region Save/Load Personal Ghosts
        
        /// <summary>
        /// Save ghost data to personal folder
        /// </summary>
        public static bool SaveGhost(string routeKey, GhostData data)
        {
            try
            {
                EnsureFoldersExist();
                string filePath = GetPersonalGhostPath(routeKey);
                
                using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    // Header
                    writer.Write(FILE_MAGIC);
                    writer.Write(FILE_VERSION);
                    writer.Write(data.RouteName ?? "");
                    writer.Write(data.CarName ?? "");
                    writer.Write(data.TotalTime);
                    writer.Write(data.RecordedDate);
                    
                    // Frames
                    writer.Write(data.Frames.Count);
                    foreach (var frame in data.Frames)
                    {
                        writer.Write(frame.Timestamp);
                        writer.Write(frame.Position.x);
                        writer.Write(frame.Position.y);
                        writer.Write(frame.Position.z);
                        writer.Write(frame.Rotation.x);
                        writer.Write(frame.Rotation.y);
                        writer.Write(frame.Rotation.z);
                        writer.Write(frame.Rotation.w);
                        writer.Write(frame.WheelSteerAngle);
                        writer.Write(frame.Speed);
                        writer.Write(frame.EngineRPM);
                        writer.Write(frame.Gear);
                        writer.Write(frame.Flags);
                    }
                }
                
                Plugin.Log.LogInfo($"Ghost saved: {filePath} ({data.Frames.Count} frames, {data.GetTimeString()})");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to save ghost: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load ghost data from personal folder
        /// </summary>
        public static GhostData LoadGhost(string routeKey)
        {
            try
            {
                string filePath = GetPersonalGhostPath(routeKey);
                if (!File.Exists(filePath))
                    return null;
                
                return LoadGhostFromPath(filePath, isShared: false);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to load ghost: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if a personal ghost exists
        /// </summary>
        public static bool PersonalGhostExists(string routeKey)
        {
            return File.Exists(GetPersonalGhostPath(routeKey));
        }
        
        /// <summary>
        /// Delete a personal ghost
        /// </summary>
        public static bool DeleteGhost(string routeKey)
        {
            try
            {
                string filePath = GetPersonalGhostPath(routeKey);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Plugin.Log.LogInfo($"Ghost deleted: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to delete ghost: {e.Message}");
                return false;
            }
        }
        
        #endregion

        #region Shared Ghosts
        
        /// <summary>
        /// Scan shared folder for valid ghost files
        /// </summary>
        public static List<SharedGhostInfo> ScanSharedGhosts()
        {
            var sharedGhosts = new List<SharedGhostInfo>();
            
            string sharedPath = GetSharedPath();
            if (!Directory.Exists(sharedPath))
                return sharedGhosts;
            
            string[] files = Directory.GetFiles(sharedPath, "*.ghost");
            
            // Security: Limit number of files
            if (files.Length > MAX_SHARED_GHOST_FILES)
            {
                Plugin.Log.LogWarning($"Security: Too many files in shared folder ({files.Length}), only processing first {MAX_SHARED_GHOST_FILES}");
                files = files.Take(MAX_SHARED_GHOST_FILES).ToArray();
            }
            
            foreach (string filePath in files)
            {
                var info = ValidateSharedGhost(filePath);
                if (info != null && info.IsValid)
                {
                    sharedGhosts.Add(info);
                    Plugin.Log.LogInfo($"Found shared ghost: {info.PlayerName} on {info.RouteKey} ({info.TimeString})");
                }
            }
            
            Plugin.Log.LogInfo($"Scanned shared ghosts: {sharedGhosts.Count} valid files found");
            return sharedGhosts;
        }
        
        /// <summary>
        /// Validate a shared ghost file and extract metadata
        /// </summary>
        public static SharedGhostInfo ValidateSharedGhost(string filePath)
        {
            var info = new SharedGhostInfo
            {
                FilePath = filePath,
                IsValid = false
            };
            
            try
            {
                // Security: Check path traversal
                if (!IsPathSafe(filePath, GetSharedPath()))
                {
                    Plugin.Log.LogWarning("Shared ghost rejected: path traversal detected");
                    return info;
                }
                
                // Security: Check file size
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MAX_GHOST_FILE_SIZE)
                {
                    Plugin.Log.LogWarning($"Shared ghost rejected: file too large ({fileInfo.Length} bytes)");
                    return info;
                }
                
                if (fileInfo.Length < 20)
                {
                    Plugin.Log.LogWarning("Shared ghost rejected: file too small");
                    return info;
                }
                
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    // Check magic
                    string magic = SafeReadString(reader, 10);
                    if (magic != FILE_MAGIC)
                        return info;
                    
                    // Check version
                    int version = reader.ReadInt32();
                    if (version != FILE_VERSION)
                        return info;
                    
                    // Read route name
                    string routeName = SafeReadString(reader, MAX_STRING_LENGTH);
                    if (string.IsNullOrEmpty(routeName))
                    {
                        Plugin.Log.LogWarning("Shared ghost rejected: empty route name");
                        return info;
                    }
                    
                    // Check if route exists
                    if (!Routes.All.ContainsKey(routeName))
                    {
                        Plugin.Log.LogWarning($"Shared ghost has unknown route: {routeName}");
                        return info;
                    }
                    
                    info.RouteKey = routeName;
                    
                    // Skip car name
                    SafeReadString(reader, MAX_STRING_LENGTH);
                    
                    // Get player name from filename
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    info.PlayerName = SanitizePlayerName(fileName);
                    
                    // Read total time
                    info.TotalTime = SafeReadFloat(reader, MIN_VALID_TIME, MAX_VALID_TIME);
                    
                    int minutes = (int)(info.TotalTime / 60);
                    float seconds = info.TotalTime % 60;
                    info.TimeString = $"{minutes}:{seconds:00.000}";
                    
                    // Skip recordedDate
                    reader.ReadInt64();
                    
                    // Check frame count
                    int frameCount = reader.ReadInt32();
                    if (frameCount <= 0 || frameCount > MAX_FRAME_COUNT)
                    {
                        Plugin.Log.LogWarning($"Shared ghost rejected: invalid frame count ({frameCount})");
                        return info;
                    }
                    
                    info.IsValid = true;
                }
            }
            catch (InvalidDataException e)
            {
                Plugin.Log.LogWarning($"Shared ghost rejected (malformed): {e.Message}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Failed to validate shared ghost {filePath}: {e.Message}");
            }
            
            return info;
        }
        
        /// <summary>
        /// Load ghost data from a shared ghost file
        /// </summary>
        public static GhostData LoadSharedGhost(SharedGhostInfo info)
        {
            if (info == null || !info.IsValid)
                return null;
            
            return LoadGhostFromPath(info.FilePath, isShared: true);
        }
        
        #endregion

        #region Internal Load Methods
        
        private static GhostData LoadGhostFromPath(string filePath, bool isShared)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;
                
                // Security for shared files
                if (isShared)
                {
                    if (!IsPathSafe(filePath, GetSharedPath()))
                    {
                        Plugin.Log.LogError("Security: path traversal blocked");
                        return null;
                    }
                    
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > MAX_GHOST_FILE_SIZE)
                    {
                        Plugin.Log.LogError($"Security: file too large ({fileInfo.Length} bytes)");
                        return null;
                    }
                }
                
                GhostData data = new GhostData();
                
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    // Header
                    string magic = isShared ? SafeReadString(reader, 10) : reader.ReadString();
                    if (magic != FILE_MAGIC)
                    {
                        Plugin.Log.LogError("Invalid ghost file format");
                        return null;
                    }
                    
                    int version = reader.ReadInt32();
                    if (version != FILE_VERSION)
                    {
                        Plugin.Log.LogError($"Unsupported ghost version: {version}");
                        return null;
                    }
                    
                    if (isShared)
                    {
                        data.RouteName = SafeReadString(reader, MAX_STRING_LENGTH);
                        data.CarName = SafeReadString(reader, MAX_STRING_LENGTH);
                        data.TotalTime = SafeReadFloat(reader, MIN_VALID_TIME, MAX_VALID_TIME);
                    }
                    else
                    {
                        data.RouteName = reader.ReadString();
                        data.CarName = reader.ReadString();
                        data.TotalTime = reader.ReadSingle();
                    }
                    
                    data.RecordedDate = reader.ReadInt64();
                    
                    // Frames
                    int frameCount = reader.ReadInt32();
                    if (isShared && (frameCount <= 0 || frameCount > MAX_FRAME_COUNT))
                    {
                        Plugin.Log.LogError($"Security: invalid frame count ({frameCount})");
                        return null;
                    }
                    
                    data.Frames = new List<GhostFrame>(frameCount);
                    
                    const float MAX_POS = 50000f;
                    float lastTimestamp = -1f;
                    
                    for (int i = 0; i < frameCount; i++)
                    {
                        GhostFrame frame = new GhostFrame();
                        
                        if (isShared)
                        {
                            frame.Timestamp = SafeReadFloat(reader, 0f, MAX_VALID_TIME);
                            
                            // Ensure monotonic timestamps
                            if (frame.Timestamp < lastTimestamp)
                                frame.Timestamp = lastTimestamp + 0.001f;
                            lastTimestamp = frame.Timestamp;
                            
                            float px = SafeReadFloat(reader, -MAX_POS, MAX_POS);
                            float py = SafeReadFloat(reader, -MAX_POS, MAX_POS);
                            float pz = SafeReadFloat(reader, -MAX_POS, MAX_POS);
                            frame.Position = new Vector3(px, py, pz);
                            
                            float qx = SafeReadFloat(reader, -1.5f, 1.5f);
                            float qy = SafeReadFloat(reader, -1.5f, 1.5f);
                            float qz = SafeReadFloat(reader, -1.5f, 1.5f);
                            float qw = SafeReadFloat(reader, -1.5f, 1.5f);
                            frame.Rotation = new Quaternion(qx, qy, qz, qw);
                            
                            frame.WheelSteerAngle = SafeReadFloat(reader, -90f, 90f);
                            frame.Speed = SafeReadFloat(reader, -500f, 500f);
                            frame.EngineRPM = SafeReadFloat(reader, 0f, 20000f);
                            
                            int gear = reader.ReadInt32();
                            frame.Gear = (gear < -1 || gear > 10) ? 0 : gear;
                        }
                        else
                        {
                            frame.Timestamp = reader.ReadSingle();
                            frame.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            frame.Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            frame.WheelSteerAngle = reader.ReadSingle();
                            frame.Speed = reader.ReadSingle();
                            frame.EngineRPM = reader.ReadSingle();
                            frame.Gear = reader.ReadInt32();
                        }
                        
                        frame.Flags = reader.ReadByte();
                        data.Frames.Add(frame);
                    }
                }
                
                Plugin.Log.LogInfo($"Loaded ghost: {data.Frames.Count} frames, {data.GetTimeString()}");
                return data;
            }
            catch (InvalidDataException e)
            {
                Plugin.Log.LogError($"Security: malformed ghost file - {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to load ghost from path: {e.Message}");
                return null;
            }
        }
        
        #endregion

        #region Security Helpers
        
        private static bool IsPathSafe(string filePath, string allowedFolder)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    Plugin.Log.LogWarning($"Security: symlink/reparse point rejected: {filePath}");
                    return false;
                }
                
                string fullPath = Path.GetFullPath(filePath);
                string allowedPath = Path.GetFullPath(allowedFolder);
                
                if (!fullPath.StartsWith(allowedPath, StringComparison.OrdinalIgnoreCase))
                {
                    Plugin.Log.LogWarning($"Security: path outside allowed folder rejected: {filePath}");
                    return false;
                }
                
                string relativePath = fullPath.Substring(allowedPath.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    
                if (relativePath.Contains("..") || 
                    relativePath.Contains(Path.DirectorySeparatorChar) || 
                    relativePath.Contains(Path.AltDirectorySeparatorChar))
                {
                    Plugin.Log.LogWarning($"Security: subdirectory/traversal rejected: {filePath}");
                    return false;
                }
                
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Security: path validation failed: {e.Message}");
                return false;
            }
        }
        
        private static string SafeReadString(BinaryReader reader, int maxLength)
        {
            int length = 0;
            int shift = 0;
            byte b;
            
            do
            {
                if (shift >= 35)
                    throw new InvalidDataException("Invalid string length encoding");
                    
                b = reader.ReadByte();
                length |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            
            if (length < 0 || length > maxLength)
                throw new InvalidDataException($"String length {length} exceeds maximum {maxLength}");
            
            if (length == 0)
                return "";
            
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
                throw new InvalidDataException("Unexpected end of file reading string");
                
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        
        private static float SafeReadFloat(BinaryReader reader, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            float value = reader.ReadSingle();
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException("Invalid float value (NaN or Infinity)");
            if (value < minVal || value > maxVal)
                throw new InvalidDataException($"Float value {value} out of valid range");
            return value;
        }
        
        private static string SanitizePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unknown";
            
            var sanitized = new System.Text.StringBuilder();
            
            foreach (char c in name)
            {
                if (char.IsControl(c))
                    continue;
                
                var category = char.GetUnicodeCategory(c);
                if (category == System.Globalization.UnicodeCategory.Format ||
                    category == System.Globalization.UnicodeCategory.PrivateUse ||
                    category == System.Globalization.UnicodeCategory.Surrogate ||
                    category == System.Globalization.UnicodeCategory.OtherNotAssigned)
                    continue;
                
                if (c == '<' || c == '>' || c == '"' || c == '\'' || c == '\\' || c == '/' ||
                    c == '\r' || c == '\n' || c == '\t' || c == '\0')
                    continue;
                
                sanitized.Append(c);
                
                if (sanitized.Length >= 32)
                    break;
            }
            
            string result = sanitized.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? "Unknown" : result;
        }
        
        #endregion
    }
}
