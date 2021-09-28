using UnityEngine;

namespace MovementAlgorithms {
    public class Arrive : SteeringBehaviour {

        // Public set so that the arrive target can be changed at runtime
        public KinematicData target;

        public float slowRadius;

        public float stopRadius;

        public float timeToTarget;

        public float maxSpeed;

        public Arrive(KinematicData target, float slowRadius, float stopRadius, float timeToTarget, float maxSpeed, float maxLinearAcceleration) : base(maxLinearAcceleration) {
            this.target = target;
            this.slowRadius = slowRadius;
            this.stopRadius = stopRadius;
            this.timeToTarget = timeToTarget;
            this.maxSpeed = maxSpeed;
        }

        public override SteeringOutput GetSteering(KinematicData character) {
            
            SteeringOutput steering = new SteeringOutput();
            
            Vector3 direction = target.position - character.position;
            float distanceToTarget = direction.magnitude;

            // If we are within the stop radius we stop accelerating
            if (distanceToTarget <= stopRadius)
                return SteeringOutput.zero;

            float targetSpeed;

            // If we are within the slow radius then the speed depends on the distance to the target
            if (distanceToTarget <= slowRadius)
                targetSpeed = maxSpeed * distanceToTarget / slowRadius;
            else // else we use max speed
                targetSpeed = maxSpeed;

            Vector3 targetVelocity = direction.normalized * targetSpeed;
                
            steering.linearAcceleration = (targetVelocity - character.linearVelocity) / timeToTarget;    

            // If the target linear acceleration is above the maximum then we set it to the maximum
            if (steering.linearAcceleration.magnitude > maxLinearAcceleration)
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxLinearAcceleration;

            steering.angularAcceleration = 0.0f;        

            return steering;
        }
    }
}