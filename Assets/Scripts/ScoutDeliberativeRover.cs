using System;
using UnityEngine;
using MovementAlgorithms;
using System.Collections.Generic;

[Serializable]
struct ScoutDeliberativeParameters {
    public float environmentSensorRadius;

    [Range(1, 120)]
    public float rotationSpeed;

    [Range(0, 15)]
    public int numberOfEnvironmentSensors;
    public float maxGasoline;
    public float minGasoline;
    public int missChance;
    public int fuelExpenditure;
    public int antennaRange;
    public float breakProbability;
}
public class ScoutDeliberativeRover : DeliberativeRover
{

    [SerializeField]
    private DeliberativeMessage messageToBroadcast = DeliberativeMessage.NONE;
    [SerializeField]
    private DeliberativeMessage receivedMessage = DeliberativeMessage.NONE;
    public bool MAYDAY = false;

    [SerializeField]
    private ScoutDeliberativeParameters scoutParameters;
    [SerializeField]
    private DeliberativeMovementParameters movementParameters;
    private List<DeliberativeMessage> messageQueue = new List<DeliberativeMessage>();
    void Start() {
        base.InitMovementAlgorithms(movementParameters);

        _environmentSensorsAngles = new float[scoutParameters.numberOfEnvironmentSensors];

        float radiusDiff = 360.0f/scoutParameters.numberOfEnvironmentSensors;
        for (int i = 0; i < _environmentSensorsAngles.Length; i++)
        {   
            _environmentSensorsAngles[i] = i * radiusDiff;
        }
        base.fuelExpenditure = scoutParameters.fuelExpenditure;
        base.currentFuel = scoutParameters.maxGasoline;
        base.antennaRange = scoutParameters.antennaRange;
        base.breakProbability = scoutParameters.breakProbability;
    }

    void FixedUpdate() {
        base.exchangeTimer += Time.fixedDeltaTime;
        if (base.exchangeTimer > base.timeBetweenExchange) {
            base.ExchangeMaps();
            base.exchangeTimer = 0.0f;
        }
        MakeDecisions();
    }

    private void MakeDecisions() {

        // Refill tank when in base
        if (base.IsNearPoint(World.baseCamp.position, baseRange)) {
            RefillFuel();
        }

        FuelState state = CheckFuel();
        // If fuel is empty, broadcast no fuel and stay still
        if (state.Equals(FuelState.EMPTY)) {
            MAYDAY = true;
            BroadcastMessage(new DeliberativeMessage(this, DeliberativeMessageType.I_AM_BROKEN));
            MapPoint mp = new MapPoint(PointType.BROKEN_ROVER, this);
            InsertPointInMap(mp);
            return;
        // If fuel state is low move towards base to refill
        } else if (state.Equals(FuelState.LOW)) {
            MAYDAY = false;
            // Debug.Log("Scout Fuel is low, moving towards base!");
            MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.transform.position);
            if (!messageToBroadcast.Equals(DeliberativeMessage.NONE)) {
                BroadcastMessage(messageToBroadcast);
            }
            return;
        }

        MAYDAY = false;

        if (CheckMessage()) {
            switch (receivedMessage.type) {
                case DeliberativeMessageType.I_AM_BROKEN:
                    if (messageToBroadcast.Equals(DeliberativeMessage.NONE)) {
                        MapPoint mp = new MapPoint(PointType.BROKEN_ROVER, receivedMessage.sender);
                        UpdateMap(mp);
                        receivedMessage = DeliberativeMessage.NONE;
                    }
                    else {
                        receivedMessage = DeliberativeMessage.NONE;
                    }
                    break;
                default:
                    receivedMessage = DeliberativeMessage.NONE;
                    break;
            } 
            receivedMessage = DeliberativeMessage.NONE;
        }

        EnvironmentSensorInfo sensorInfo;
        if (SenseEnvironment(out sensorInfo)) {
            // ADD RESOURCE TO MAP if its not there
            UpdateMap(new MapPoint(PointType.RESOURCE, sensorInfo.resource));
        }

        // ALWAYS MOVE RANDOM, UNLESS GAS IS LOW
        base.MoveRandomly(Time.fixedDeltaTime);
    }

    #region ACTUATORS
    public override void RefillFuel() {
        currentFuel = scoutParameters.maxGasoline;
    }

    #endregion

    #region SENSORS
    private float[] _environmentSensorsAngles; // Contains the current angle of each of the sensors
    public override bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo) {
        RaycastHit[] raycastHits = null;

        // Stops as soon as a ray hits something
        for (int i = 0; i < _environmentSensorsAngles.Length; i++)
        {
            _environmentSensorsAngles[i] += scoutParameters.rotationSpeed * Time.deltaTime;

            _environmentSensorsAngles[i] %= 360.0f;
            
            Quaternion rayCastRotation = Quaternion.AngleAxis(_environmentSensorsAngles[i], transform.up);
            Vector3 rayCastDir = rayCastRotation * transform.forward;
            Vector3 rayCastOrigin = transform.position + transform.up; // We move the origin slightly up to be above the ground

            Debug.DrawRay(rayCastOrigin, rayCastDir.normalized * scoutParameters.environmentSensorRadius, Color.blue);
            
            raycastHits = Physics.RaycastAll(rayCastOrigin, rayCastDir, scoutParameters.environmentSensorRadius);
            
            for (int j = 0; j < raycastHits.Length; j++)
            {
                string tag = raycastHits[j].collider.tag;
                GameObject resource = raycastHits[j].collider.gameObject;
                Markable markable = resource.GetComponent<Markable>();
                // If the resource has already been marked by another rover then
                // we ignore it
                if (markable != null && !markable.isMarked){
                    int miss = UnityEngine.Random.Range(0,100);
                    markable.MarkResource();
                    switch (tag) {
                        case "SmallResource" : {
                            if (miss < scoutParameters.missChance/2) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.MEDIUM, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else if (miss < scoutParameters.missChance) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.LARGE, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else {
                            sensorInfo.resource = new Resource(
                                ResourceType.SMALL, 
                                raycastHits[j].transform.position,
                                resource
                            );
                            sensorInfo.detectedResource = true;
                            }
                            return true;
                        }
                        case "MediumResource" : {
                            if (miss < scoutParameters.missChance/2) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.SMALL, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else if (miss < scoutParameters.missChance) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.LARGE, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else {
                            sensorInfo.resource = new Resource(
                                ResourceType.MEDIUM, 
                                raycastHits[j].transform.position,
                                resource
                            );
                            sensorInfo.detectedResource = true;
                            }
                            return true;
                        }
                        case "LargeResource" : {
                            if (miss < scoutParameters.missChance/2) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.MEDIUM, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else if (miss < scoutParameters.missChance) {
                                sensorInfo.resource = new Resource(
                                    ResourceType.SMALL, 
                                    raycastHits[j].transform.position,
                                    resource
                                );
                                sensorInfo.detectedResource = true;
                            }
                            else {
                            sensorInfo.resource = new Resource(
                                ResourceType.LARGE, 
                                raycastHits[j].transform.position,
                                resource
                            );
                            sensorInfo.detectedResource = true;
                            }
                            return true;
                        }
                    }
                }
            }
        }
        sensorInfo = EnvironmentSensorInfo.EMPTY;
        return false;
    }

    public override FuelState CheckFuel() {
        if (currentFuel <= 0)
            return FuelState.EMPTY;
        if (currentFuel <= scoutParameters.minGasoline)
            return FuelState.LOW;
        return FuelState.NORMAL;
    }

    // Checks if he received any Message
    public override bool CheckMessage() {
        return (!receivedMessage.Equals(DeliberativeMessage.NONE));
    }
    public override void ReceiveMessage(DeliberativeMessage message){
        // messageQueue.Add(message);

        if (receivedMessage.Equals(DeliberativeMessage.NONE) && 
            (message.type.Equals(DeliberativeMessageType.MAP_UPDATES) || 
            message.type.Equals(DeliberativeMessageType.I_AM_BROKEN)))
            receivedMessage = message;
    }

    #endregion

    public Stack<Action<DeliberativeRover>> GeneratePlan() {
        return null;
    }
}