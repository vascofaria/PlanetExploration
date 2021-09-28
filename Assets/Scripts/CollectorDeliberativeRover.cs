using System;
using UnityEngine;
using MovementAlgorithms;
using System.Collections.Generic;

[Serializable]
struct CollectorDeliberativeParameters {
    public float maxGasoline;
    public float minGasoline;
    public int fuelExpenditure;
    public int antennaRange;
    public float breakProbability;
}
public class CollectorDeliberativeRover : DeliberativeRover
{
    [SerializeField]
    public DeliberativeMessage messageToBroadcast = DeliberativeMessage.NONE;
    [SerializeField]
    public DeliberativeMessage receivedMessage = DeliberativeMessage.NONE;
    [SerializeField]
    public Resource targetResource = Resource.NONE;

    [SerializeField]
    public List<MapPoint> smallTargetResource = new List<MapPoint>();
    [SerializeField]
    public bool carryingGas = false;
    [SerializeField]
    public DeliberativeRover targetRover;
    public Vector3 targetPosition;
    [SerializeField]
    public Resource heldResource = Resource.NONE;
    private bool actionDone = false;
    public bool planDone =true;
    [SerializeField]
    private Stack<Action<CollectorDeliberativeRover>> roverPlan = new Stack<Action<CollectorDeliberativeRover>>();
    public Stack<PlanAction> plan = new Stack<PlanAction>();
    public List<Resource> heldSmallResouces = new List<Resource>();
    public Transform resourcePosition;
    public GameObject gasCan;
    public bool MAYDAY = false;
    [SerializeField]
    public bool waitingForRover { set { _waitingForRover = value; Debug.Log("Set called with: " + value); } get {return _waitingForRover; }}
    private bool _waitingForRover = false;
    [SerializeField]
    private CollectorDeliberativeParameters collectorParameters;
    [SerializeField]
    public DeliberativeMovementParameters movementParameters;
    private List<DeliberativeMessage> messageQueue = new List<DeliberativeMessage>();
    void Start() {
        base.InitMovementAlgorithms(movementParameters);

        base.fuelExpenditure = collectorParameters.fuelExpenditure;
        base.currentFuel = collectorParameters.maxGasoline;
        base.antennaRange = collectorParameters.antennaRange;
        base.breakProbability = collectorParameters.breakProbability;
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
        // If fuel is empty then we broadcast I am broken and stay still
        if (state.Equals(FuelState.EMPTY)) {
            MAYDAY = true;
            BroadcastMessage(new DeliberativeMessage(this, DeliberativeMessageType.I_AM_BROKEN));
            MapPoint mp = new MapPoint(PointType.BROKEN_ROVER, this);
            InsertPointInMap(mp);
            return;
        // If fuel state is low move towards base to refill
        } else if (state.Equals(FuelState.LOW)) {
            MAYDAY = false;
            // Debug.Log("Collector Fuel is low, moving towards base!");
            MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            return;
        }
        MAYDAY = false;

        // -----------------------------------------------------------------------------
        // TODO PLAN MAKING
        // MAKE A PLAN, AND FOLLOW IT UNTIL IT BELIEVES IS NO LONGER POSSIBLE TO ACHIEVE
        // -----------------------------------------------------------------------------

        if (planDone) {
            plan = GeneratePlan();
        }

        if (plan.Count > 0 && plan.Peek().done) {
            if(plan.Peek().type.Equals(ActionType.GRAB_SMALL_RESOURCE)){
                Debug.Log(this.name + " Pop" + plan.Peek().type+" Resource "+plan.Peek().targetRs.resource.name + "Pos" + this.position);
            }
            Debug.Log(this.name + " Pop" + plan.Peek().type);
            plan.Pop();
        }

        if (plan.Count > 0) {
            Action<CollectorDeliberativeRover> actualAction = plan.Peek().action;
            actualAction(this);
        } else {
            planDone = true;
        }
    }

    public bool isCloser(Vector3 currentPoint, Vector3 newPoint) {
        return (this.position - newPoint).sqrMagnitude < (this.position - currentPoint).sqrMagnitude;
    }

    #region ACTUATORS
    //Tem de receber gameobject para poder interagir com o resource no mapa. Maybe mudar o Resource.cs
    public void GrabResource(Resource resource) {
        //resource.resource.SetActive(false);
        resource.resource.transform.position = resourcePosition.position;
        Destroy(resource.resource.GetComponent<PlanetGravityBody>());
        resource.resource.transform.parent = this.transform;
        Destroy(resource.resource.GetComponent<Rigidbody>());
        heldResource = resource;
        targetResource = Resource.NONE;
    }

    public void GrabSmallResource(Resource resource) {
        resource.resource.transform.position = resourcePosition.position;
        Destroy(resource.resource.GetComponent<PlanetGravityBody>());
        resource.resource.transform.parent = this.transform;
        Destroy(resource.resource.GetComponent<Rigidbody>());
        heldSmallResouces.Add(resource);
        //smallTargetResource.RemoveAt(smallTargetResource.Count-1);
    }
    public void DropResourceBase(Resource resource) { 
        World.baseCamp.StoreResource(resource);
        resource.resource.SetActive(false);
        heldResource = Resource.NONE;
        targetResource = Resource.NONE;
    }

    public void DropSmallResourceBase(List<Resource> resources) {
        foreach(Resource resource in resources){ 
            World.baseCamp.StoreResource(resource);
            resource.resource.SetActive(false);
        }
        heldSmallResouces.Clear();
        smallTargetResource.Clear(); 
    }

    public void DropResourceEnvironment(Resource resource) {
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
    public override FuelState CheckFuel(){
        if (currentFuel <= 0)
            return FuelState.EMPTY;
        if (currentFuel <= collectorParameters.minGasoline)
            return FuelState.LOW;
        return FuelState.NORMAL;
    }
    public override bool SenseEnvironment(out EnvironmentSensorInfo sensorInfo) { sensorInfo = EnvironmentSensorInfo.EMPTY; return false;}
    public override bool CheckMessage() {
        return (!receivedMessage.Equals(DeliberativeMessage.NONE));
    }
    public override void ReceiveMessage(DeliberativeMessage message) {
       if (receivedMessage.Equals(DeliberativeMessage.NONE) && (
            message.type.Equals(DeliberativeMessageType.HELP_CARRY_RESOURCE) ||
            message.type.Equals(DeliberativeMessageType.LETS_CARRY_TOGETHER) ||
            message.type.Equals(DeliberativeMessageType.MAP_UPDATES)))
        receivedMessage = message;
    }
    #endregion

    private bool HasCargo() {
        return carryingGas || !heldResource.Equals(Resource.NONE);
    }

    public Stack<PlanAction> GeneratePlan() {
        if (map.Count == 0) return new Stack<PlanAction>();

        Stack<PlanAction> newPlan = new Stack<PlanAction>();
        List<MapPoint> smallResourcesMapPoints = new List<MapPoint>();
        MapPoint mediumResourceMapPoint = MapPoint.NONE;
        MapPoint largeResourceMapPoint = MapPoint.NONE;
        MapPoint brokenRoverMapPoint = MapPoint.NONE;

        planDone = false;
        Debug.Log("Generating");
        List<MapPoint> smallList = new List<MapPoint>();
        List<MapPoint> mediumList = new List<MapPoint>();
        List<MapPoint> RoverList = new List<MapPoint>();


        foreach(MapPoint mapPoint in map) {
            if (!mapPoint.assigned && !mapPoint.wasCompleted) {
                // Se e small resource e no plano temos menos de 3 small resources add small resource
                if (mapPoint.type.Equals(PointType.RESOURCE)) {
                    switch (mapPoint.resource.resourceType) {
                        case ResourceType.SMALL:
                            smallList.Add(mapPoint);
                            break;
                        case ResourceType.MEDIUM:
                            mediumList.Add(mapPoint);
                            break;
                        case ResourceType.LARGE:
                            if (largeResourceMapPoint.Equals(MapPoint.NONE) ||
                                isCloser(largeResourceMapPoint.position, mapPoint.resource.position)
                            ) {
                                largeResourceMapPoint = mapPoint;
                            }
                            break;
                    }

                } else if (mapPoint.type.Equals(PointType.BROKEN_ROVER)) {
                    RoverList.Add(mapPoint);
                }
            }
        }
        if(mediumList.Count>0)
            mediumResourceMapPoint = mediumList[UnityEngine.Random.Range(0,mediumList.Count-1)];
        if(RoverList.Count>0)
            brokenRoverMapPoint = RoverList[UnityEngine.Random.Range(0,RoverList.Count-1)];
        if(smallList.Count>0){
            if(smallList.Count<3){
                foreach(MapPoint small in smallList){
                    smallResourcesMapPoints.Add(small);
                }
            }
            else{
                int rand1 = UnityEngine.Random.Range(0,smallList.Count-1);
                smallResourcesMapPoints.Add(smallList[rand1]);
                smallList.RemoveAt(rand1);
                int rand2 = UnityEngine.Random.Range(0,smallList.Count-1);
                smallResourcesMapPoints.Add(smallList[rand2]);
                smallList.RemoveAt(rand2);
                int rand3 = UnityEngine.Random.Range(0,smallList.Count-1);
                smallResourcesMapPoints.Add(smallList[rand3]);
                smallList.RemoveAt(rand3);
            }
        }
        if (!brokenRoverMapPoint.Equals(MapPoint.NONE)) {
            newPlan.Push(new PlanAction(ActionType.HELP_ROVER));
            newPlan.Push(new PlanAction(ActionType.GO_TO_ROVER));
            newPlan.Push(new PlanAction(ActionType.GRAB_GAS));
            newPlan.Push(new PlanAction(ActionType.GO_TO_BASE));

            targetRover = brokenRoverMapPoint.brokenRover;
            targetPosition = brokenRoverMapPoint.position;
            // Update MapPoint
            brokenRoverMapPoint.assigned = this;
            brokenRoverMapPoint.utility = 20;
            UpdateMap(brokenRoverMapPoint);

        } else if (!largeResourceMapPoint.Equals(MapPoint.NONE)) {
            newPlan.Push(new PlanAction(ActionType.DROP_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.GO_TO_BASE));
            newPlan.Push(new PlanAction(ActionType.GRAB_LARGE_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.WAIT_FOR_ROVER));
            newPlan.Push(new PlanAction(ActionType.GO_TO_RESOURCE));

            if (largeResourceMapPoint.waitingRover != null) {
                largeResourceMapPoint.assigned = this;
            } else {
                largeResourceMapPoint.waitingRover = this;
            }
            targetResource = largeResourceMapPoint.resource;
            targetPosition = largeResourceMapPoint.position;
            // Update MapPoint
            largeResourceMapPoint.utility = 15;
            UpdateMap(largeResourceMapPoint);

        } else if (smallResourcesMapPoints.Count == 3) {
            newPlan.Push(new PlanAction(ActionType.DROP_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.GO_TO_BASE));

            for (int i = smallResourcesMapPoints.Count - 1; i >= 0 ; i--)
            {   
                newPlan.Push(PlanAction.GetGrabSmallResourceAction(smallResourcesMapPoints[i].resource, smallResourcesMapPoints[i].position));
                newPlan.Push(PlanAction.GetGoToPositionAction(smallResourcesMapPoints[i].position,smallResourcesMapPoints[i].resource));
            }

            for(int i = smallResourcesMapPoints.Count-1 ; i>=0;i--){
                MapPoint smallResourceMp = smallResourcesMapPoints[i]; 
                smallResourceMp.assigned = this;
                smallResourceMp.utility = 4*smallResourcesMapPoints.Count;
                UpdateMap(smallResourceMp);
            }

        } else if (!mediumResourceMapPoint.Equals(MapPoint.NONE)) {
            newPlan.Push(new PlanAction(ActionType.DROP_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.GO_TO_BASE));
            newPlan.Push(new PlanAction(ActionType.GRAB_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.GO_TO_RESOURCE));

            mediumResourceMapPoint.assigned = this;
            targetResource = mediumResourceMapPoint.resource;
            targetPosition = mediumResourceMapPoint.position;
            // Update MapPoint
            mediumResourceMapPoint.utility = 10;
            UpdateMap(mediumResourceMapPoint);
        } else if (smallResourcesMapPoints.Count > 0) {
            newPlan.Push(new PlanAction(ActionType.DROP_RESOURCE));
            newPlan.Push(new PlanAction(ActionType.GO_TO_BASE));

            for (int i = smallResourcesMapPoints.Count - 1; i >= 0 ; i--)
            {   
                newPlan.Push(PlanAction.GetGrabSmallResourceAction(smallResourcesMapPoints[i].resource, smallResourcesMapPoints[i].position));
                newPlan.Push(PlanAction.GetGoToPositionAction(smallResourcesMapPoints[i].position,smallResourcesMapPoints[i].resource));
            }
            
            for(int i = smallResourcesMapPoints.Count-1 ; i>=0;i--){
                MapPoint smallResourceMp = smallResourcesMapPoints[i]; 
                smallResourceMp.assigned = this;
                smallResourceMp.utility = 4*smallResourcesMapPoints.Count;
                UpdateMap(smallResourceMp);
            }
        
        } else {
            planDone = true;
        }

        return newPlan;
    }
}
