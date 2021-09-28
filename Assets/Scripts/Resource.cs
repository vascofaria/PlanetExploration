using UnityEngine;
using System;

public enum ResourceType {
    SMALL,
    MEDIUM,
    LARGE
}

[Serializable]
public struct Resource {
    public ResourceType resourceType;

    public GameObject resource;

    public Vector3 position => resource.transform.position;

    public uint numberOfRoversGrabbingResource;

    public static Resource NONE = new Resource();

    public Resource(ResourceType resourceType, Vector3 position, GameObject resource) {
        this.resourceType = resourceType;
        //this.position = position;
        this.resource = resource;
        this.numberOfRoversGrabbingResource = 0;
    }

    public void CountGrabGlobally() {
        for (int i = 0; i < World.resources.Count; i++)
        {
            if (World.resources[i].resourceType == resourceType && World.resources[i].position == position) {
                Resource resource = World.resources[i];
                resource.numberOfRoversGrabbingResource++;
                World.resources[i] = resource;
                numberOfRoversGrabbingResource = resource.numberOfRoversGrabbingResource;
                return;
            }
        }
    }

    public Resource GetResourceGlobally() {
        for (int i = 0; i < World.resources.Count; i++)
        {
            if (World.resources[i].resourceType == resourceType && World.resources[i].position == position) {
                return World.resources[i];
            }
        } 
        return Resource.NONE;
    }
}