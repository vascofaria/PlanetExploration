using UnityEngine;

namespace MovementAlgorithms {
    public class ArriveAvoidObstacle : Arrive {

        public float whiskersAngleOffset;

        public float raySizes;

        public float avoidDistance;

        public ArriveAvoidObstacle(KinematicData target, float whiskersAngleOffset,
        float raySizes, float avoidDistance, float slowRadius, float stopRadius, float timeToTarget, float maxSpeed, float maxLinearAcceleration) : base(target, slowRadius, stopRadius, timeToTarget, maxSpeed, maxLinearAcceleration) {
            this.whiskersAngleOffset = whiskersAngleOffset;
            this.raySizes = raySizes;
            this.avoidDistance = avoidDistance;
        }

        public override SteeringOutput GetSteering(KinematicData character) {
            
            Vector3 centerRaycastDir = target.position - character.position;

            Vector3 leftRaycastDir = Quaternion.AngleAxis(-whiskersAngleOffset,character.up) * centerRaycastDir;

            Vector3 rightRaycastDir = Quaternion.AngleAxis(whiskersAngleOffset,character.up) * centerRaycastDir; 

            Debug.DrawRay(character.position + character.up, centerRaycastDir.normalized * raySizes / 3.0f, Color.green);
            Debug.DrawRay(character.position+ character.up, leftRaycastDir.normalized * raySizes/ 3.0f, Color.green);
            Debug.DrawRay(character.position+ character.up, rightRaycastDir.normalized * raySizes, Color.green);

            RaycastHit hitPoint;

            if (Physics.Raycast(character.position+ character.up, leftRaycastDir.normalized * raySizes/ 3.0f, out hitPoint) || 
            Physics.Raycast(character.position+ character.up, rightRaycastDir.normalized * raySizes/ 3.0f, out hitPoint) ||
            Physics.Raycast(character.position+ character.up, centerRaycastDir.normalized * raySizes, out hitPoint)) {
                
                float distanceToTarget = (target.position - character.position).magnitude;
                target.position = hitPoint.point - character.up + hitPoint.normal * avoidDistance;
                target.position = character.position + (target.position - character.position).normalized * distanceToTarget * 0.8f;
            } 

            return base.GetSteering(character);
        }
    }
}