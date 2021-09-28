using UnityEngine;


namespace MovementAlgorithms {
    class LookWhereYouAreGoing : Align {

        public LookWhereYouAreGoing(
            float slowRadius,
            float timeToLook, 
            float maxAngularVelocity,
            float maxAngularAcceleration, 
            float maxLinearAcceleration) 
            : base(
            new KinematicData(), 
            slowRadius, 
            timeToLook, 
            maxAngularVelocity, 
            maxLinearAcceleration,
            maxLinearAcceleration) { }

        public override SteeringOutput GetSteering(KinematicData character) {
            
            // If the character does not have any velocity then it should not look
            if (character.linearVelocity == Vector3.zero) 
                return SteeringOutput.zero;

            // We set our target orientation to the direction of the velocity
            target.orientation = character.linearVelocity.normalized;

            return base.GetSteering(character);
        }
    }

}