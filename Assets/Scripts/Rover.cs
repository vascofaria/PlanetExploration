using System;
using System.Collections.Generic;
using UnityEngine;
using MovementAlgorithms;

public enum FuelState {
    NORMAL,
    LOW,
    EMPTY
}

[Serializable]
public struct MovementParameters {
    [Header("General parameters")]
    public float maxSpeed;
    public float maxLinearAcceleration;

    [Header("LookWhereYouAreGoing parameters")]
    public float slowRadius;
    public float timeToLook; 
    public float maxAngularVelocity;
    public float maxAngularAcceleration;

    [Header("Wander parameters")]
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRateInDegrees;

    [Header("Arrive parameters")]
    public float arriveSlowRadius;
    public float stopRadius;
    public float timeToTarget;
    [Header("Avoid obstacle parameters")]
    public float whiskersAngleOffset;
    public float raySizes;
    public float avoidDistance;
}

[RequireComponent(typeof(Rigidbody))]
public abstract class Rover : MonoBehaviour
{
    private LookWhereYouAreGoing _lookWhereYouAreGoing;
    private Wander _wander;
    private SphereArrive _sphereArrive;
    private SphereDynamicCharacter _dynamicCharacter;
    // private int seed = 1;
    public Vector3 position => transform.position;

    [Header("Rover Info")]
    [SerializeField]
    protected float currentFuel = 100.0f;
    protected int antennaRange;     //Different for each type
    protected int fuelExpenditure;     //Different for each type
    protected float breakProbability;
    [SerializeField]
    protected float baseRange = 15;
    [SerializeField]
    [Tooltip("Time until it stops broadcasting a message in seconds")]
    protected float broadCastTimeout = 90.0f; 
    [SerializeField]
    protected float currBroadcastTime = 0.0f;
    protected void InitMovementAlgorithms(MovementParameters movementParameters) {
        _lookWhereYouAreGoing = new LookWhereYouAreGoing(
            movementParameters.slowRadius,
            movementParameters.timeToLook,
            movementParameters.maxAngularVelocity,
            movementParameters.maxAngularAcceleration,
            movementParameters.maxLinearAcceleration
        );
        _wander = new Wander(
            movementParameters.wanderOffset,
            movementParameters.wanderRadius,
            movementParameters.wanderRateInDegrees,
            movementParameters.maxLinearAcceleration
        );
        _sphereArrive = new SphereArrive(
            KinematicData.zero,
            GameObject.Find("Planet").transform.position,
            movementParameters.arriveSlowRadius,
            movementParameters.stopRadius,
            movementParameters.timeToTarget,
            movementParameters.maxSpeed,
            movementParameters.maxLinearAcceleration
        );

        _dynamicCharacter = new SphereDynamicCharacter(GetComponent<Rigidbody>(), movementParameters.maxSpeed, movementParameters.maxAngularVelocity);
    }

    #region ACTUATORS
    protected void MoveRandomly(float fixedDeltaTime) {

        if (currentFuel > 0 ){

            // First we obtain the steering output that will change the position of the character
            SteeringOutput steering = _wander.GetSteering(_dynamicCharacter.kinematicData); 

            // Integrating the output into the dynamic character
            _dynamicCharacter.IntegrateSteeringOuput(steering, fixedDeltaTime);

            // Now we obtain the steering output that will make the character face its movement direction
            steering = _lookWhereYouAreGoing.GetSteering(_dynamicCharacter.kinematicData);

            // Integrating the output into the dynamic character
            _dynamicCharacter.IntegrateSteeringOuput(steering, fixedDeltaTime);

            //Decrement fuel expended
            float fuelReduction = fixedDeltaTime*fuelExpenditure;
            currentFuel -= fuelReduction;
            UIManager.uIManager.CountGasolineSpent(fuelReduction);

            // Small chance of losing all fuel
            RandomBreak();
        } else {
            Debug.Log("No gasoline " + this.gameObject.name);
        }
    }
    protected void MoveTowardsPoint(float fixedDeltaTime, Vector3 destination) {

        if (currentFuel > 0 ) {

            _sphereArrive.targetDestination = new KinematicData() { position = destination };

            // First we obtain the steering output that will change the position of the character
            SteeringOutput steering = _sphereArrive.GetSteering(_dynamicCharacter.kinematicData);

            // Integrating the output into the dynamic character
            _dynamicCharacter.IntegrateSteeringOuput(steering, fixedDeltaTime);

            // Now we obtain the steering output that will make the character face its movement direction
            steering = _lookWhereYouAreGoing.GetSteering(_dynamicCharacter.kinematicData);

            // Integrating the output into the dynamic character
            _dynamicCharacter.IntegrateSteeringOuput(steering, fixedDeltaTime);
           
            //Decrement fuel expended
            float fuelReduction = fixedDeltaTime*fuelExpenditure;
            currentFuel -= fuelReduction;
            UIManager.uIManager.CountGasolineSpent(fuelReduction);

            // Small chance of losing all fuel
            RandomBreak();
        } else {
            Debug.Log("No gasoline " + this.gameObject.name);
        }

    }
    protected void BroadcastMessage(Message message) { 
        foreach(Rover r in GetRoversInRange()){
            if (!r.Equals(this))
                r.ReceiveMessage(message);
        }
    }
    protected void SendMessage(ScoutRover scoutRover, Message message) {  
        scoutRover.ReceiveMessage(message);
    }
    protected void SendMessage(CollectorRover collectorRover, Message message) {  
        collectorRover.ReceiveMessage(message);
    }
    protected void SendMessage(Rover rover, Message message) {  
        rover.ReceiveMessage(message);
    }
    public abstract void RefillFuel();
    #endregion

    #region SENSORS
    protected abstract FuelState CheckFuel();
    public abstract bool CheckMessage();

    public abstract bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo);

    public abstract void ReceiveMessage(Message message);

    protected bool IsNearPoint(Vector3 point, float checkRadius) {
        return (point - transform.position).magnitude <= checkRadius;
    }

    #endregion

    protected List<Rover> GetRoversInRange() {
        List<Rover> rovers_near = new List<Rover>();
        foreach (Rover r in World.rovers) {
            if ((r.transform.position - transform.position).magnitude <= antennaRange) {
                rovers_near.Add(r);
            }
        }
        return rovers_near;
    }

    private void RandomBreak() {
        if (UnityEngine.Random.Range(0.0f, 100.0f) < breakProbability) {
            Debug.Log("MAYDAY");
            UnityEngine.Random.seed = (UnityEngine.Random.seed + 1) % 5000;
            // UnityEngine.Random.InitState(seed);
            // seed = (seed + 1) % 50;
            // UnityEngine.Random.seed += 1;
            currentFuel = 0;
        }
    }

    public bool IsBroken() {
        return currentFuel <= 0;
    }
}