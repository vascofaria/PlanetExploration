using UnityEngine;

namespace MovementAlgorithms {
    public struct KinematicData {
        
        // Static data
        public Vector3 position;
        public Vector3 orientation;

        // The local up and forward axis in world space
        public Vector3 up;
        public Vector3 forward;
        public Vector3 right;

        // Actual kinematic data
        public Vector3 linearVelocity;

        public float angularVelocity; // in degrees/second 

        public readonly static KinematicData zero = new KinematicData() { position = Vector3.zero, orientation = Vector3.zero, linearVelocity = Vector3.zero, angularVelocity = 0.0f };
    }
}