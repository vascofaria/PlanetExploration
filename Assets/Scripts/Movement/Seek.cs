
using UnityEngine;

namespace MovementAlgorithms {
    public class Seek : SteeringBehaviour {

        // Public set so that the seek target can be changed at runtime
        public KinematicData target;

        public Seek(KinematicData target, float maxLinearAcceleration) : base(maxLinearAcceleration) {
            this.target = target;
        }

        public override SteeringOutput GetSteering(KinematicData character) {
            
            SteeringOutput steering = new SteeringOutput();
            
            Vector3 direction = target.position - character.position;
            
            // Accelerate at max acceleration towards target
            steering.linearAcceleration = direction.normalized * maxLinearAcceleration;
            steering.angularAcceleration = 0.0f;

            //Debug.DrawRay(character.position, direction, Color.blue);

            return steering;
        }


    }
}