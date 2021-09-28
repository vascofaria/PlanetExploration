
using UnityEngine;

namespace MovementAlgorithms {
    public class Wander : Seek {

        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;

        public Wander(float wanderOffset, float wanderRadius, float wanderRate, float maxLinearAcceleration) : base(new KinematicData(), maxLinearAcceleration)  {
            this.wanderOffset = wanderOffset;
            this.wanderRadius = wanderRadius;
            this.wanderRate = wanderRate;
        }

        public override SteeringOutput GetSteering(KinematicData character) {
        

            // Calculate the center of the wander circle 
            target.position = character.position + wanderOffset * character.forward;

            // Calculate the wander orientation
            float wanderOrientation = wanderRate * MathExtensions.RandomBinomial();

            // Calculate the target position within the wander circle perimeter
            Quaternion targetOrientation = Quaternion.AngleAxis(wanderOrientation, character.up);
            
            // Debug rays that allow us to visualize the directions of each of the wander vectors
            //Debug.DrawRay(character.position, target.position - character.position, Color.green);

             // Rotating the character orientation to point to the target orientation
            Vector3 targetForward = targetOrientation * character.orientation;

            //Debug.DrawRay(target.position, targetForward.normalized * wanderRadius, Color.red);
            
            target.position += wanderRadius * targetForward.normalized;
            
            // Finally seek towards target
            return base.GetSteering(character);
        }
    }
}