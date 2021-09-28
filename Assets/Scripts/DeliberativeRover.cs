using System;
using System.Collections.Generic;
using UnityEngine;
using MovementAlgorithms;

[Serializable]
public struct DeliberativeMovementParameters {
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
public abstract class DeliberativeRover : MonoBehaviour
{
    private LookWhereYouAreGoing _lookWhereYouAreGoing;
    private Wander _wander;
    private SphereArrive _sphereArrive;
    private SphereDynamicCharacter _dynamicCharacter;
    // private int seed = 1;
    public Vector3 position => transform.position;

    [Header("Rover Info")]
    [SerializeField]
    protected float currentFuel;
    protected int antennaRange;     //Different for each type
    protected int fuelExpenditure;     //Different for each type
    protected float breakProbability;
    [SerializeField]
    public float baseRange = 15;
    [SerializeField]
    [Tooltip("Time until it stops broadcasting a message in seconds")]
    protected float broadCastTimeout = 90.0f; 
    [SerializeField]
    protected float currBroadcastTime = 0.0f;
    [SerializeField]
    protected List<MapPoint> map = new List<MapPoint>();
    // A VECTOR OF IMPORTANTE POINTS SUCH AS RESOURCES, BROKEN_ROVERS AND THE ONES THAT ARE CURRENTLY HANDLING THAT TASKS
    public float exchangeTimer = 0.0f;

    public float timeBetweenExchange = 2f;
    protected void InitMovementAlgorithms(DeliberativeMovementParameters movementParameters) {
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
    public void MoveRandomly(float fixedDeltaTime) {

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
    public void MoveTowardsPoint(float fixedDeltaTime, Vector3 destination) {

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
    public void BroadcastMessage(DeliberativeMessage message) { 
        foreach(DeliberativeRover r in GetRoversInRange()){
            if (!r.Equals(this))
                r.ReceiveMessage(message);
        }
    }
    public void SendMessage(ScoutDeliberativeRover scoutRover, DeliberativeMessage message) {  
        scoutRover.ReceiveMessage(message);
    }
    public void SendMessage(CollectorDeliberativeRover collectorRover, DeliberativeMessage message) {  
        collectorRover.ReceiveMessage(message);
    }
    public void SendMessage(DeliberativeRover rover, DeliberativeMessage message) {  
        rover.ReceiveMessage(message);
    }
    public void ExchangeMaps() {
        Debug.Log("Exchanging Maps");
        List<MapPoint> newMap = map;
        MapPoint found;
        List<DeliberativeRover> roversInRange = GetRoversInRange();
        foreach (DeliberativeRover rover in roversInRange) {
            if (rover != this) {
                foreach(MapPoint mapPoint in rover.map) {
                    
                    found = newMap.Find((mp) => { 
                        return mp.type.Equals(mapPoint.type) && isVectorClose(mp.position, mapPoint.position);
                    });

                    if (found.Equals(MapPoint.NONE)) {
                        newMap.Add(mapPoint);
                    } else {
                        if (found.Equals(mapPoint))
                            continue;

                        if (mapPoint.wasCompleted && !found.wasCompleted || 
                        mapPoint.assigned != null && found.assigned == null) {
                            newMap.Remove(found);
                            newMap.Add(mapPoint);
                            continue;
                        }

                        if (mapPoint.waitingRover != null && found.waitingRover != null) {
                            if (!mapPoint.waitingRover.Equals(found.waitingRover)) {
                                found.waitingRover = mapPoint.waitingRover;
                                found.assigned = this;
                            }
                        }

                        if (mapPoint.assigned != found.assigned && 
                        mapPoint.assigned != null && found.assigned != null &&
                        mapPoint.utility > found.utility) {
                            newMap.Remove(found);
                            newMap.Add(mapPoint);
                        }
                    }
                }
            }
        }
        map = newMap;
    }
    public void UpdateMap(MapPoint mapPoint) {
        foreach (MapPoint mp in map) 
        {   
            // If map already contains this point, update it
            if (mp.type.Equals(mapPoint.type) && isVectorClose(mp.position, mapPoint.position)) {
                map.Remove(mp);
                map.Add(mapPoint);
                return;
            }
        }

        // Map does not contain point so add it
        map.Add(mapPoint);
    }
    public void InsertPointInMap(MapPoint mapPoint) {
        foreach (MapPoint mp in map) 
        {
            // If map already contains this point, update it
            if (mp.type.Equals(mapPoint.type) && isVectorClose(mp.position, mapPoint.position)) {
                return;
            }
        }

        // Map does not contain point so add it
        map.Add(mapPoint);
    }
    private bool isVectorClose(Vector3 first, Vector3 second) {
        return (first-second).sqrMagnitude <= 0.1;
    }
    public abstract void RefillFuel();
    #endregion

    #region SENSORS
    public abstract FuelState CheckFuel();
    public abstract bool CheckMessage();

    public abstract bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo);

    public abstract void ReceiveMessage(DeliberativeMessage message);

    public bool IsNearPoint(Vector3 point, float checkRadius) {
        Vector3 cena = (point - this.transform.position);
        return cena.sqrMagnitude <= checkRadius*checkRadius;
    }

    #endregion

    protected List<DeliberativeRover> GetRoversInRange() {
        List<DeliberativeRover> rovers_near = new List<DeliberativeRover>();
        foreach (DeliberativeRover r in World.deliberativeRovers) {
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
    //public abstract Stack<Action<DeliberativeRover>> GeneratePlan();    
}