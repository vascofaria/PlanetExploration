using System.Collections.Generic;

public static class World {
    public static List<Rover> rovers = new List<Rover>();
    public static List<DeliberativeRover> deliberativeRovers = new List<DeliberativeRover>();
    public static BaseCamp baseCamp;
    public static List<Resource> resources = new List<Resource>();
    public static void Reset() {
        World.rovers = new List<Rover>();
        World.deliberativeRovers = new List<DeliberativeRover>();
        World.resources = new List<Resource>();
    }
}