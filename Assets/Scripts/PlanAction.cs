using System;
using UnityEngine;

public enum ActionType {
    WAIT,
    GO_TO_BASE,
    GO_TO_ROVER,
    GRAB_GAS,
    HELP_ROVER,
    GO_TO_RESOURCE,
    GO_TO_SMALL_RESOURCE,
    GRAB_RESOURCE,
    DROP_RESOURCE,
    WAIT_FOR_ROVER,
    GRAB_LARGE_RESOURCE,
    GRAB_SMALL_RESOURCE
}

public class PlanAction
{
    public bool done = false;

    public ActionType type;
    public Resource targetRs;
    public Vector3 targetPosition;

    public Action<CollectorDeliberativeRover> action;

    public PlanAction(Action<CollectorDeliberativeRover> action) {
        this.action = action;
    }

    public PlanAction(ActionType type) {
        this.type = type;
        switch (type) {
            case ActionType.GO_TO_BASE:
                this.action = this.getGoToBaseAction();
                break;
            
            case ActionType.GO_TO_RESOURCE:
                this.action = this.getGotToResourceAction();
                break;

            case ActionType.GRAB_RESOURCE:
                this.action = this.getGrabResourceAction();
                break;

            case ActionType.DROP_RESOURCE:
                this.action = this.getDropResourceAction();
                break;

            case ActionType.GRAB_GAS:
                this.action = this.getGrabGasAction();
                break;

            case ActionType.GO_TO_ROVER:
                this.action = this.getGoToRoverAction();
                break;
            
            case ActionType.HELP_ROVER:
                this.action = this.getHelpRoverAction();
                break;

            case ActionType.WAIT_FOR_ROVER:
                this.action = this.getWaitForRoverAction();
                break;
            
            case ActionType.GRAB_LARGE_RESOURCE:
                this.action = this.getGrabLargeResourceAction();
                break;
        }
    }

    public PlanAction() {}

    public static PlanAction GetGrabSmallResourceAction(Resource targetResource, Vector3 targetPosition) {
        PlanAction action = new PlanAction();
        action.action = action.getGrabSmallResourceAction();
        action.type = ActionType.GRAB_SMALL_RESOURCE;
        action.targetRs = targetResource;
        action.targetPosition = targetPosition;
        return action;
    }
    
    public static PlanAction GetGoToPositionAction(Vector3 targetPosition,Resource targetResource) {
        PlanAction action = new PlanAction();
        action.action = action.getGoToSmallResourceAction();
        action.type = ActionType.GO_TO_SMALL_RESOURCE;
        action.targetPosition = targetPosition;
        action.targetRs = targetResource;
        return action;
    }
    
    private Action<CollectorDeliberativeRover> getGoToBaseAction() {
        return ((rover) => {
            if (!rover.IsNearPoint(World.baseCamp.position, rover.baseRange)) {
                rover.MoveTowardsPoint(Time.fixedDeltaTime, World.baseCamp.position);
            } else {
                UIManager.uIManager.CountTripsToBase(1);
                this.done = true;
            }
        });
    }
    private Action<CollectorDeliberativeRover> getGotToResourceAction() {
        return ((rover) => {
            if (!rover.targetResource.Equals(Resource.NONE)) {
                if (!rover.IsNearPoint(rover.targetPosition, rover.movementParameters.stopRadius)) {
                    rover.MoveTowardsPoint(Time.fixedDeltaTime, rover.targetPosition);
                } else {
                    this.done = true;
                }
            }
        });
    }
    
    private Action<CollectorDeliberativeRover> getGoToSmallResourceAction() {
        return ((rover) => {
            if (!targetRs.Equals(Resource.NONE)) {
                if (!rover.IsNearPoint(targetPosition, rover.movementParameters.stopRadius)) {
                    rover.MoveTowardsPoint(Time.fixedDeltaTime, targetPosition);
                } else {
                    this.done = true;
                }
            }
        });
    }
    private Action<CollectorDeliberativeRover> getGrabSmallResourceAction() {
        return ((rover) => {
            if (rover.IsNearPoint(targetPosition, rover.movementParameters.stopRadius) &&
                 isVectorClose(targetPosition, targetRs.position)){
                    rover.GrabSmallResource(targetRs);
            } else {
                UIManager.uIManager.CountUselessTrips(1);
            }
            this.done = true;
        });
    }
    private Action<CollectorDeliberativeRover> getGrabResourceAction() {
        return ((rover) => {
            if (rover.IsNearPoint(rover.targetPosition, rover.movementParameters.stopRadius) &&
                 isVectorClose(rover.targetPosition, rover.targetResource.position)){
                    rover.GrabResource(rover.targetResource);
            } else {
                UIManager.uIManager.CountUselessTrips(1);
                AbandonPlan(rover);
            }
            this.done = true;
        });
    }

    private Action<CollectorDeliberativeRover> getDropResourceAction() {
        return ((rover) => {
            if (rover.heldSmallResouces.Count > 0) {
                rover.DropSmallResourceBase(rover.heldSmallResouces);
            } else if(!rover.heldResource.Equals(Resource.NONE)){
                rover.DropResourceBase(rover.heldResource);
            }

            this.done = true;
        });
    }

    private Action<CollectorDeliberativeRover> getGrabGasAction() {
        return ((rover) => {
            rover.carryingGas = true;
            rover.gasCan.SetActive(true);
            this.done = true;
        });
    }

    private Action<CollectorDeliberativeRover> getGoToRoverAction() {
        return ((rover) => {
            if (!rover.IsNearPoint(rover.targetPosition, rover.baseRange)) {
                rover.MoveTowardsPoint(Time.fixedDeltaTime, rover.targetPosition);
            } else {
                this.done = true;
            }
        });
    }

    private Action<CollectorDeliberativeRover> getHelpRoverAction() {
        return ((rover) => {
            if (rover.targetRover.IsBroken()) {
                rover.targetRover.RefillFuel();
                rover.targetRover = null;
                rover.targetPosition = new Vector3(0,0,0);
                rover.carryingGas = false;
                rover.gasCan.SetActive(false);
            } else {
                UIManager.uIManager.CountUselessTrips(1);
                AbandonPlan(rover);
                rover.carryingGas = false;
                rover.gasCan.SetActive(false);
                // TODO What happens to the gas??
            }
            this.done = true;
        });
    }

    private Action<CollectorDeliberativeRover> getWaitForRoverAction() {
        return ((rover) => {

            Resource globalResource = rover.targetResource.GetResourceGlobally();
            
            if (rover.waitingForRover) {
                if (globalResource.numberOfRoversGrabbingResource == 2) {
                    if(isVectorClose(rover.targetPosition,rover.targetResource.position)){
                        this.done = true;
                        rover.heldResource = rover.targetResource;
                        rover.waitingForRover = false;
                    }
                    else{
                        UIManager.uIManager.CountUselessTrips(1);
                        rover.targetResource = Resource.NONE;
                        rover.targetPosition = new Vector3(0,0,0);
                        AbandonPlan(rover);
                    }
                } // else wait
            } else if (globalResource.numberOfRoversGrabbingResource == 0) {
                rover.targetResource.CountGrabGlobally();
                rover.waitingForRover = true;
            } else if (globalResource.numberOfRoversGrabbingResource == 1) {
                rover.targetResource.CountGrabGlobally();
                rover.heldResource = rover.targetResource;
                rover.plan.Pop();
                rover.plan.Pop();
                rover.waitingForRover = false;
                this.done = true;
            } else {
                UIManager.uIManager.CountUselessTrips(1);
                rover.targetResource = Resource.NONE;
                rover.targetPosition = new Vector3(0,0,0);
                AbandonPlan(rover);
            }
        });
    }

    private Action<CollectorDeliberativeRover> getGrabLargeResourceAction() {
        return ((rover) => {
            if (rover.IsNearPoint(rover.targetPosition, rover.movementParameters.stopRadius) &&
                isVectorClose(rover.targetPosition, rover.targetResource.position)) {
                    rover.GrabResource(rover.targetResource);
            }
            rover.receivedMessage = DeliberativeMessage.NONE;
            rover.targetPosition = new Vector3(0,0,0);
            this.done = true;
        });
    }

    private void AbandonPlan(CollectorDeliberativeRover rover) {
        while(rover.plan.Count > 0) {
            rover.plan.Pop();
        }
    }
    private bool isVectorClose(Vector3 first, Vector3 second) {
        return (first-second).sqrMagnitude <= 0.1;
    }
}
