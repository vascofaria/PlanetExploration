using System;
using UnityEngine;

public enum MessageType {
    NONE,
    FOUND_BROKEN_ROVER,
    I_AM_BROKEN,
    AVAILABLE_TO_HELP_ROVER,
    GO_TO_ROVER,
    FOUND_RESOURCE,
    AVAILABLE_TO_COLLECT_RESOURCE,
    GO_TO_RESOURCE,
    HELP_CARRY_RESOURCE
}

[Serializable]
public struct Message {
    public Rover sender;
    public MessageType type;
    public Resource resource;
    public Rover brokenRover;
    public Rover rover;
    public Vector3 brokenRoverPosition;
    public static Message NONE = new Message();

    public Message(Rover sender, MessageType type) {
        this.sender = sender;
        this.type = type;
        this.resource = Resource.NONE;
        this.rover = null;
        this.brokenRover = null;
        this.brokenRoverPosition = new Vector3(0,0,0);
    }

    public Message(Rover sender, MessageType type, Rover brokenRover) {
        this.sender = sender;
        this.type = type;
        this.resource = Resource.NONE;
        this.rover = null;
        this.brokenRover = brokenRover;
        this.brokenRoverPosition = brokenRover.position;

    }

    public Message(Rover sender, MessageType type, Resource resource) {
        this.sender = sender;
        this.type = type;
        this.resource = resource;
        this.rover = null;
        this.brokenRover = null;
        this.brokenRoverPosition = new Vector3(0,0,0);
    }

    public Message(Rover sender, MessageType type, Resource resource, Rover other) {
        this.sender = sender;
        this.type = type;
        this.resource = resource;
        this.rover = other;
        this.brokenRover = null;
        this.brokenRoverPosition = new Vector3(0,0,0);
    }

}