using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DeliberativeMessageType {
    NONE,
    I_AM_BROKEN,
    MAP_UPDATES,
    HELP_CARRY_RESOURCE,
    LETS_CARRY_TOGETHER
}

public struct DeliberativeMessage {
    public DeliberativeRover sender;
    public DeliberativeMessageType type;
    public Resource resource;
    public static DeliberativeMessage NONE = new DeliberativeMessage();

    public DeliberativeMessage(DeliberativeRover sender, DeliberativeMessageType type) {
        this.sender = sender;
        this.type = type;
        this.resource = Resource.NONE;
    }
    public DeliberativeMessage(DeliberativeRover sender, DeliberativeMessageType type, Resource resource) {
        this.sender = sender;
        this.type = type;
        this.resource = resource;
    }
}
