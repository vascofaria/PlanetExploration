using UnityEngine;

namespace MovementAlgorithms {
    public class SphereArrive : Arrive {
        
        // The final target of arrive in world space
        public KinematicData targetDestination;
        public Vector3 sphereCenter;

        public SphereArrive(KinematicData target, Vector3 sphereCenter, float slowRadius, float stopRadius, float timeToTarget, float maxSpeed, float maxLinearAcceleration) : 
        base(KinematicData.zero, slowRadius, stopRadius, timeToTarget, maxSpeed, maxLinearAcceleration) {
            this. targetDestination = target;
            this.sphereCenter = sphereCenter;
        }

        public override SteeringOutput GetSteering(KinematicData character) {

            float distanceToTarget = (character.position - targetDestination.position).magnitude;
            
            Vector3 centerToTarget = targetDestination.position - sphereCenter;
            Vector3 centerToCharacter = character.position - sphereCenter;

            Vector3 orthogonal = Vector3.Cross(centerToTarget, centerToCharacter);

            Vector3 direction = Vector3.Cross(centerToCharacter,orthogonal);
            
            // Arrive expects position, not the direction so by adding our position to the direction we the the target position
            target.position = character.position + direction.normalized * distanceToTarget;

            return base.GetSteering(character);
        }
    }
}