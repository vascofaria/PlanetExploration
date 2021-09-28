
using UnityEngine;

namespace MovementAlgorithms {

    /*
    Align tries to match the orientation of the character with that of the target. It pays no
    attention to the position or velocity of the character or target. It tries to reach the target 
    orientation and tries to have zero angular velocity when it gets there.
    */
    public class Align : SteeringBehaviour {

        public KinematicData target;
        public float slowRadius { get; private set; }
        public float timeToTarget {get; private set; }
        public float maxAngularVelocity { get; private set; }
        public float maxAngularAcceleration { get; private set; }
        

        public Align(KinematicData target, float slowRadius, float timeToTarget, float maxAngularVelocity, float maxAngularAcceleration, float maxLinearAcceleration) : base(maxLinearAcceleration) {
            this.target = target;
            this.slowRadius = slowRadius;
            this.timeToTarget = timeToTarget;
            this.maxAngularVelocity = maxAngularVelocity;
            this.maxAngularAcceleration = maxAngularAcceleration;
        }

        public override SteeringOutput GetSteering(KinematicData character) {
            
            SteeringOutput steering = new SteeringOutput();

            // First we calculate the rotation direction
            // The returned angle is in the [-180,180] interval
            float rotationDirection = Vector3.SignedAngle(character.orientation, target.orientation, character.up);

            // The target orientation is the rotation direction
            float rotationSize = Mathf.Abs(rotationDirection);

            // The closer the orientations of the character and target the smaller the target angular velocity
            // unless its outside the slow radius because then it we aim for maximimum angular velocity
            float targetAngularVelocity = rotationSize > slowRadius ?
                maxAngularVelocity : (maxAngularVelocity * rotationSize) / slowRadius;
            targetAngularVelocity *= Mathf.Sign(rotationDirection);

            // Calculating angular acceleration to reach target angular velocity
            steering.angularAcceleration = (targetAngularVelocity - character.angularVelocity) / timeToTarget;

            if (Mathf.Abs(steering.angularAcceleration) > maxAngularAcceleration) 
                steering.angularAcceleration = maxAngularAcceleration * Mathf.Sign(steering.angularAcceleration);

            steering.linearAcceleration = Vector3.zero;

            return steering;
        }
    } 
}