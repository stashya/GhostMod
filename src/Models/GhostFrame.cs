using System;
using System.Reflection;
using UnityEngine;

namespace GhostMod.Models
{
    /// <summary>
    /// Represents a single frame of ghost data
    /// Contains position, rotation, and vehicle state
    /// </summary>
    [Serializable]
    public class GhostFrame
    {
        public float Timestamp;
        public Vector3 Position;
        public Quaternion Rotation;
        public float WheelSteerAngle;
        public float Speed;
        public float EngineRPM;
        public int Gear;
        public byte Flags; // Bit 0: brake, Bit 1: NOS, Bit 2: headlights

        // Flag accessors
        public bool IsBraking => (Flags & 1) != 0;
        public bool IsUsingNOS => (Flags & 2) != 0;
        public bool HasHeadlights => (Flags & 4) != 0;

        /// <summary>
        /// Default constructor for deserialization
        /// </summary>
        public GhostFrame() { }

        /// <summary>
        /// Create a frame from current car state
        /// </summary>
        public GhostFrame(float time, Transform carTransform, RCC_CarControllerV3 car)
        {
            Timestamp = time;
            Position = carTransform.position;
            Rotation = carTransform.rotation;
            
            // Get wheel steer angle from front wheel
            if (car.FrontLeftWheelCollider != null)
                WheelSteerAngle = car.FrontLeftWheelCollider.wheelCollider.steerAngle;
            
            Speed = car.speed;
            
            // engineRPM is internal, use reflection
            var engineRPMField = typeof(RCC_CarControllerV3).GetField("engineRPM", 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (engineRPMField != null)
                EngineRPM = (float)engineRPMField.GetValue(car);
            
            Gear = car.currentGear;
            
            // Pack flags
            Flags = 0;
            if (car.brakeInput > 0.1f) Flags |= 1;
            if (car.useNOS && car.boostInput > 0.1f) Flags |= 2;
            if (car.lowBeamHeadLightsOn) Flags |= 4;
        }
    }
}
