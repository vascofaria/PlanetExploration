using UnityEngine;

namespace MovementAlgorithms {
    /*
    Used in dynamic movement algorithms whose output is
    the desired acceleration
    */
    public struct SteeringOutput {
        public Vector3 linearAcceleration;
        public float angularAcceleration; // in degrees

        public readonly static SteeringOutput zero = new SteeringOutput() { linearAcceleration = Vector3.zero, angularAcceleration = 0.0f };

        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b) => new SteeringOutput() { linearAcceleration=a.linearAcceleration + b.linearAcceleration, angularAcceleration = a.angularAcceleration + b.angularAcceleration};
    }
}