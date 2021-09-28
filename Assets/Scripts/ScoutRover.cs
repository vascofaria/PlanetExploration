using System;
using UnityEngine;
using MovementAlgorithms;
using System.Collections.Generic;

[Serializable]
struct ScoutParameters {
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
public class ScoutRover : Rover
{

    [SerializeField]
    private Message messageToBroadcast = Message.NONE;
    [SerializeField]
    private Message receivedMessage = Message.NONE;

    public bool MAYDAY = false;

    [SerializeField]
    private ScoutParameters scoutParameters;
    [SerializeField]
    private MovementParameters movementParameters;

    private Rover waitingRover = null;

    private List<Message> messageQueue = new List<Message>();
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
            BroadcastMessage(new Message(this, MessageType.I_AM_BROKEN));
            return;
        // If fuel state is low move towards base to refill
        } else if (state.Equals(FuelState.LOW)) {
            MAYDAY = false;
            // Debug.Log("Scout Fuel is low, moving towards base!");
            MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.transform.position);
            if (!messageToBroadcast.Equals(Message.NONE)) {
                BroadcastMessage(messageToBroadcast);
                currBroadcastTime += Time.fixedDeltaTime;
            }
            return;
        }

        MAYDAY = false;

        if (CheckMessage()) {
            switch (receivedMessage.type) {
                case MessageType.AVAILABLE_TO_COLLECT_RESOURCE:
                    if (!messageToBroadcast.Equals(Message.NONE)) {

                        Message newMessage = new Message(this, MessageType.GO_TO_RESOURCE, messageToBroadcast.resource);
                        
                        if (messageToBroadcast.resource.resourceType.Equals(ResourceType.LARGE)) {
                            if (waitingRover != null) {
                                newMessage.rover = waitingRover;
                                waitingRover = null;
                                messageToBroadcast = Message.NONE;
                            } else 
                                waitingRover = receivedMessage.sender;
                        } else {
                            messageToBroadcast = Message.NONE;
                        }

                        SendMessage(receivedMessage.sender, newMessage);
                        currBroadcastTime = 0.0f;
                        receivedMessage = Message.NONE;

                    } else {
                        receivedMessage = Message.NONE;
                    }
                    break;
                case MessageType.AVAILABLE_TO_HELP_ROVER:
                    if (!messageToBroadcast.Equals(Message.NONE) && messageToBroadcast.type.Equals(MessageType.FOUND_BROKEN_ROVER)) {
                        if(messageToBroadcast.brokenRover==null)
                            print("enas");
                        Message newMessage = new Message(this, MessageType.GO_TO_ROVER, messageToBroadcast.brokenRover);
                        SendMessage(receivedMessage.sender, newMessage);
                        messageToBroadcast = Message.NONE;
                        currBroadcastTime = 0.0f;
                        receivedMessage = Message.NONE;
                    }
                    else {
                        receivedMessage = Message.NONE;
                    }
                    break;
                case MessageType.I_AM_BROKEN:
                    if (messageToBroadcast.Equals(Message.NONE)) {
                        messageToBroadcast = new Message(this, MessageType.FOUND_BROKEN_ROVER, receivedMessage.sender);
                        currBroadcastTime = 0.0f;
                        receivedMessage = Message.NONE;
                    }
                    else {
                        receivedMessage = Message.NONE;
                    }
                    break;
                default:
                    receivedMessage = Message.NONE;
                    break;
            } 

            receivedMessage = Message.NONE;
        }

        // If not broadcasting and find a resource, broadcast it
        EnvironmentSensorInfo sensorInfo;
        if (messageToBroadcast.Equals(Message.NONE) && SenseEnvironment(out sensorInfo)) {
            Message newMessage = new Message(this, MessageType.FOUND_RESOURCE, sensorInfo.resource);
            messageToBroadcast = newMessage;
        }

        // has Message to Broadcast, broadcast it while going to base
        if (!messageToBroadcast.Equals(Message.NONE)) {
            currBroadcastTime += Time.fixedDeltaTime;
            BroadcastMessage(messageToBroadcast);
            if (!base.IsNearPoint(World.baseCamp.position, baseRange)) {
                base.MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            }
        }
        // else move randomly
        else {
            base.MoveRandomly(Time.fixedDeltaTime);
        }

        // No collector is receiving the message, something went wrong, maybe rovers are broken
        /*if (currBroadcastTime >= broadCastTimeout) {
            // Unmark resource
            if (messageToBroadcast.type.Equals(MessageType.FOUND_RESOURCE))
                messageToBroadcast.resource.resource.GetComponent<Markable>().isMarked = false;
            messageToBroadcast = Message.NONE;
            currBroadcastTime = 0.0f;
        }*/
    }

    #region ACTUATORS
    public override void RefillFuel() {
        currentFuel = scoutParameters.maxGasoline;
    }
    #endregion

    #region SENSORS
    private float[] _environmentSensorsAngles; // Contains the current angle of each of the sensors
    public override bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo) {
        // TODO After a while, the sensors stop working properly
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

    protected override FuelState CheckFuel() {
        if (currentFuel <= 0)
            return FuelState.EMPTY;
        if (currentFuel <= scoutParameters.minGasoline)
            return FuelState.LOW;
        return FuelState.NORMAL;
    }

    // Checks if he received any Message
    public override bool CheckMessage() {
        return (!receivedMessage.Equals(Message.NONE));
    }
    public override void ReceiveMessage(Message message){
        // messageQueue.Add(message);

        if (message.type.Equals(MessageType.I_AM_BROKEN) && messageToBroadcast.type.Equals(MessageType.FOUND_BROKEN_ROVER))
            return;

        if (receivedMessage.Equals(Message.NONE) && 
            (message.type.Equals(MessageType.AVAILABLE_TO_COLLECT_RESOURCE) || 
            message.type.Equals(MessageType.I_AM_BROKEN) ||
            message.type.Equals(MessageType.AVAILABLE_TO_HELP_ROVER)))
            receivedMessage = message;
    }

    #endregion
}