

using UnityEngine;

namespace MovementAlgorithms {

    /*
    A character that has a rigidbody and is affected by steering outputs
    */
    public class SphereDynamicCharacter {

        private Rigidbody _rb;

        // Static data
        public Vector3 position { get => _rb.position; }
        
        // The orientation is represented by the forward vector in world space
        public Vector3 orientation { get => _rb.transform.forward; } 

        // The local up, forward and right axis in world space
        public Vector3 up { get => _rb.transform.up; }
        public Vector3 forward { get => _rb.transform.forward; }
        public Vector3 right { get => _rb.transform.right; }

        // Actual kinematic data
        public Vector3 linearVelocity = Vector3.zero;
        public float angularVelocity = 0.0f; // in degrees/second 

        private float _maxSpeed;
        private float _maxAngularVelocity;

        public KinematicData kinematicData { get => new KinematicData() {
            position = this.position,
            orientation = this.orientation,
            up = this.up,
            forward = this.forward,
            right = this.right,
            linearVelocity = this.linearVelocity,
            angularVelocity = this.angularVelocity
        }; }

        public SphereDynamicCharacter(Rigidbody rb, float maxSpeed, float maxAngularVelocity) {
            this._rb = rb;
            this._maxSpeed = maxSpeed;
            this._maxAngularVelocity = maxAngularVelocity;
        }

        /*
        Integrates the steering output in the dynamic character forward in time for deltaTime
        */
        public void IntegrateSteeringOuput(SteeringOutput so, float deltaTime) {
            
            if (so.linearAcceleration.sqrMagnitude > 0.0f) {

                Vector3 desiredDirection = so.linearAcceleration.normalized;
                
                // IMPORTANT:
                // The acceleration is being used as velocity because we are moving
                // in a sphere, if we were to add the acceleration to the current velocity
                // the resulting vector could point to unexpected directions
                Vector3 desiredVelocity = so.linearAcceleration  * deltaTime;

                if (desiredVelocity.magnitude < _maxSpeed) {
                    linearVelocity = desiredVelocity;    
                }

                if (linearVelocity.sqrMagnitude > 0.0f) {
                    Debug.DrawRay(position, linearVelocity * 10.0f, Color.blue);
                    _rb.MovePosition(position + linearVelocity);
                }
            }

            if (so.angularAcceleration != 0.0f) {
                float desiredAngularVelocity = angularVelocity + so.angularAcceleration * deltaTime;

                if (desiredAngularVelocity < _maxAngularVelocity) {
                    angularVelocity = desiredAngularVelocity;

                    // Clamping angular velocity to be betwee 360 & -360
                    angularVelocity = angularVelocity % 360.0f;
                }
                
                _rb.rotation = Quaternion.AngleAxis(angularVelocity, _rb.transform.up) * _rb.rotation;
            }
        }
    }
}