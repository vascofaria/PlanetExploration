using System;
using UnityEngine;
using MovementAlgorithms;

[Serializable]
struct CollectorParameters {
    public float maxGasoline;
    public float minGasoline;
    public int fuelExpenditure;
    public int antennaRange;
    public float breakProbability;

}
public class CollectorRover : Rover
{
    [SerializeField]
    private Message messageToBroadcast = Message.NONE;
    [SerializeField]
    private Message receivedMessage = Message.NONE;
    [SerializeField]
    private Resource targetResource = Resource.NONE;
    [SerializeField]
    private bool carryingGas = false;
    [SerializeField]
    private Rover targetRover;

    private Vector3 targetRoverPosition;
    [SerializeField]
    private Resource heldResource = Resource.NONE;
    public Transform resourcePosition;
    public GameObject gasCan;
    public bool MAYDAY = false;
    [SerializeField]
    private bool waitingForRover = false;
    [SerializeField]
    private CollectorRover otherRover;
    [SerializeField]
    private CollectorParameters collectorParameters;
    [SerializeField]
    private MovementParameters movementParameters;

    void Start() {
        base.InitMovementAlgorithms(movementParameters);

        base.fuelExpenditure = collectorParameters.fuelExpenditure;
        base.currentFuel = collectorParameters.maxGasoline;
        base.antennaRange = collectorParameters.antennaRange;
        base.breakProbability = collectorParameters.breakProbability;
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
        // If fuel is empty then we broadcast I am broken and stay still
        if (state.Equals(FuelState.EMPTY)) {
            MAYDAY = true;
            BroadcastMessage(new Message(this, MessageType.I_AM_BROKEN));
            return;
        // If fuel state is low move towards base to refill
        } else if (state.Equals(FuelState.LOW)) {
            MAYDAY = false;
            // Debug.Log("Collector Fuel is low, moving towards base!");
            MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            return;
        }
        MAYDAY = false;

        // If is holding resource move towards base or drop if in base
        if (!heldResource.Equals(Resource.NONE)) {
            if (base.IsNearPoint(World.baseCamp.position, baseRange)){
                UIManager.uIManager.CountTripsToBase(1);
                DropResourceBase(heldResource);
            } else {
                MoveTowardsPoint(Time.fixedDeltaTime,World.baseCamp.position);
            }
        // If rover needs to get resource move towards it
        } else if (waitingForRover) {
            // Only check messages if does not have cargo
            if (CheckMessage()) {
                switch (receivedMessage.type) {
                    case MessageType.HELP_CARRY_RESOURCE:
                        GrabResource(targetResource);
                        messageToBroadcast = Message.NONE;
                        receivedMessage = Message.NONE;
                        waitingForRover = false;
                        break;
                    default:
                        receivedMessage = Message.NONE;
                        break;
                }  
            }
            return; //Wait for other rover
        } else if (!targetResource.Equals(Resource.NONE)) {
            if (base.IsNearPoint(targetResource.position, movementParameters.stopRadius)){
                if (targetResource.resourceType.Equals(ResourceType.LARGE)) {
                    // First to arrive waits 
                    if (otherRover == null)
                        waitingForRover = true;
                    else {
                        heldResource = targetResource;
                        targetResource = Resource.NONE;
                        Message newMsg = new Message(this, MessageType.HELP_CARRY_RESOURCE);
                        SendMessage(otherRover, newMsg);
                    }      
                } else
                    GrabResource(targetResource);
            }
            else {
                MoveTowardsPoint(Time.fixedDeltaTime,targetResource.position);
            }
        // If is carrying gas and near vehicle drop gas
        } else if (carryingGas) {
            if (base.IsNearPoint(targetRoverPosition, baseRange)) {
                if (targetRover.IsBroken() && isVectorClose(targetRover.position,targetRoverPosition)) {
                    UIManager.uIManager.CountTripsToBase(1);
                    targetRover.RefillFuel();
                } else // If arrived to scout and is not broken then it was a useless trip
                    UIManager.uIManager.CountUselessTrips(1);
                targetRover = null;
                targetRoverPosition = new Vector3(0,0,0);
                carryingGas = false;
                gasCan.SetActive(false);
            } else {
                MoveTowardsPoint(Time.fixedDeltaTime, targetRoverPosition);
            }
        // If rover needs help but not carrying gas
        } else if (targetRover != null) {
            if (base.IsNearPoint(World.baseCamp.position, baseRange)){
                carryingGas = true;
                gasCan.SetActive(true);
            } else {
                MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            }
        } else {
            if (base.IsNearPoint(World.baseCamp.position, baseRange)){
                // Wait in base 
            } else {
                // Move towards base if away from base
                MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            }
            
            // Only check messages if does not have cargo
            if (CheckMessage()) {
                switch (receivedMessage.type) {
                    case MessageType.GO_TO_RESOURCE:
                        targetResource  = receivedMessage.resource;
                        if (receivedMessage.resource.resourceType.Equals(ResourceType.LARGE))
                            otherRover = receivedMessage.rover as CollectorRover;
                        messageToBroadcast = Message.NONE;
                        receivedMessage = Message.NONE;
                        break;
                    case MessageType.GO_TO_ROVER:
                        targetRover = receivedMessage.brokenRover;
                        targetRoverPosition = receivedMessage.brokenRoverPosition;
                        messageToBroadcast = Message.NONE;
                        receivedMessage = Message.NONE;
                        break;
                    case MessageType.FOUND_RESOURCE:
                        Message newMsg = new Message(this, MessageType.AVAILABLE_TO_COLLECT_RESOURCE);
                        SendMessage(receivedMessage.sender, newMsg);
                        messageToBroadcast = Message.NONE;
                        receivedMessage = Message.NONE;
                        break;
                    case MessageType.FOUND_BROKEN_ROVER:
                        Message newMessage = new Message(this, MessageType.AVAILABLE_TO_HELP_ROVER);
                        SendMessage(receivedMessage.sender, newMessage);
                        messageToBroadcast = Message.NONE;
                        receivedMessage = Message.NONE;
                        break;
                    default:
                        receivedMessage = Message.NONE;
                        break;
                }  
            }
        }
    }


    #region ACTUATORS
    //Tem de receber gameobject para poder interagir com o resource no mapa. Maybe mudar o Resource.cs
    void GrabResource(Resource resource) {
        //resource.resource.SetActive(false);
        resource.resource.transform.position = resourcePosition.position;
        Destroy(resource.resource.GetComponent<PlanetGravityBody>());
        resource.resource.transform.parent = this.transform;
        Destroy(resource.resource.GetComponent<Rigidbody>());
        heldResource = resource;
        targetResource = Resource.NONE;
    }

    void DropResourceBase(Resource resource) { 
        World.baseCamp.StoreResource(resource);
        resource.resource.SetActive(false);
        heldResource = Resource.NONE;
        targetResource = Resource.NONE;
    }

    void DropResourceEnvironment(Resource resource) {
        heldResource.resource.transform.position = this.position;
        heldResource.resource.SetActive(true);
        heldResource = Resource.NONE;
        targetResource = Resource.NONE;
    }

    public override void RefillFuel() {
        currentFuel = collectorParameters.maxGasoline;
    }
    #endregion

    #region SENSORS
     protected override FuelState CheckFuel(){
        if (currentFuel <= 0)
            return FuelState.EMPTY;
        if (currentFuel <= collectorParameters.minGasoline)
            return FuelState.LOW;
        return FuelState.NORMAL;
    }
    public override bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo) { sensorInfo = EnvironmentSensorInfo.EMPTY; return false;}
    public override bool CheckMessage() {
        return (!receivedMessage.Equals(Message.NONE));
    }
    public override void ReceiveMessage(Message message){
       // messageQueue.Add(message);
       if (receivedMessage.Equals(Message.NONE) && 
            message.type.Equals(MessageType.FOUND_RESOURCE) || 
            message.type.Equals(MessageType.GO_TO_RESOURCE) ||
            message.type.Equals(MessageType.GO_TO_ROVER)    ||
            message.type.Equals(MessageType.FOUND_BROKEN_ROVER) ||
            message.type.Equals(MessageType.HELP_CARRY_RESOURCE))
        receivedMessage = message;
    }
    #endregion

    private bool HasCargo() {
        return carryingGas || !heldResource.Equals(Resource.NONE);
    }

    private bool isVectorClose(Vector3 first, Vector3 second) {
        return (first-second).sqrMagnitude <= 0.1;
    }



}