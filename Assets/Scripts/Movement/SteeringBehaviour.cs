
namespace MovementAlgorithms {
    /*
    Base interface for all dynamic movement algorithms
    */
    public abstract class SteeringBehaviour {
        public float maxLinearAcceleration { get; private set; } // Common to all steering behaviours

        public SteeringBehaviour(float maxLinearAcceleration) {
            this.maxLinearAcceleration = maxLinearAcceleration;
        }

        /*
        Given the character's kinematic data it calculates the movement's steering
        output.
        */
        public abstract SteeringOutput GetSteering(KinematicData character);

    }

}