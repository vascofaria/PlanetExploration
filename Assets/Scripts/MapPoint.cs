using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointType {
    NONE,
    BROKEN_ROVER,
    RESOURCE
}

public struct MapPoint {
    public PointType type;
    public Resource resource;
    public DeliberativeRover brokenRover;
    public bool wasCompleted;
    public DeliberativeRover assigned;
    public CollectorDeliberativeRover waitingRover;
    public Vector3 position;
    public float utility;
    public static MapPoint NONE = new MapPoint();

    public MapPoint(PointType type, Resource resource) {
        this.type = type;
        this.resource = resource;
        this.brokenRover = null;
        this.assigned = null;
        this.waitingRover = null;
        this.position = resource.position;
        this.wasCompleted = false;
        this.utility = 0.0f;
    }
    public MapPoint(PointType type, DeliberativeRover brokenRover) {
        this.type = type;
        this.resource = Resource.NONE;
        this.brokenRover = brokenRover;
        this.assigned = null;
        this.waitingRover = null;
        this.position = new Vector3(brokenRover.position.x,brokenRover.position.y,brokenRover.position.z);
        this.wasCompleted = false;
        this.utility = 0.0f;
    }

    public void  setAssigned(CollectorDeliberativeRover rover){
        this.assigned = rover;
    }
    public void  setUtility(float ut){
        this.utility = ut;
    }
}