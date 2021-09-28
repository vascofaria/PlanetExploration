using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WorldGenerator : MonoBehaviour
{

    public float ignoreAngle;
    public float baseCampAngle;
    public Transform planet;
    public GameObject smallResourcePrefab, mediumResourcePrefab, largeResourcePrefab;
    public GameObject scoutRoverPrefab, collectorRoverPrefab;
    public GameObject scoutDeliberativePrefab, collectorDeliberativePrefab;
    public GameObject baseCampPrefab;
    public SceneConfigurations sceneConfigurations;

    // A bidimentional array that contains randomly placed resources
    private int[,] _resourcesMap;

    /*
    At the start of the episode, from the resource map we populate the planet
    surface with resources
    */
    void Start()
    {   
        sceneConfigurations = SharedInfo.configurations;

        planet.localScale = new Vector3(
            sceneConfigurations.planetRadius,
            sceneConfigurations.planetRadius,
            sceneConfigurations.planetRadius);

        Vector3 baseCampSpawnDirection = new Vector3(
                0.0f, 
                1.0f, 
                0.0f
            ).normalized;

        BaseCamp baseCamp = SpawnPrefabInSphereSurface(baseCampPrefab, baseCampSpawnDirection).GetComponent<BaseCamp>();
        World.baseCamp = baseCamp;

        for (int i = 0; i < sceneConfigurations.numberOfSmallResources; ++i) {
            Vector3 spawnDirection = GetSpawnDirection(baseCampSpawnDirection, ignoreAngle);


            GameObject resource = SpawnPrefabInSphereSurface(smallResourcePrefab, spawnDirection);

            World.resources.Add(new Resource(ResourceType.SMALL, 
                                    resource.transform.position,
                                    resource
                                ));
            resource.name += i.ToString();
        }

        for (int i = 0; i < sceneConfigurations.numberOfMediumResources; ++i) {
            Vector3 spawnDirection = GetSpawnDirection(baseCampSpawnDirection, ignoreAngle);


            GameObject resource = SpawnPrefabInSphereSurface(mediumResourcePrefab, spawnDirection);

            World.resources.Add(new Resource(ResourceType.MEDIUM, 
                                    resource.transform.position,
                                    resource
                                ));
            resource.name += i.ToString();
        }

        for (int i = 0; i < sceneConfigurations.numberOfLargeResources; ++i) {
            Vector3 spawnDirection = GetSpawnDirection(baseCampSpawnDirection, ignoreAngle);

            GameObject resource = SpawnPrefabInSphereSurface(largeResourcePrefab, spawnDirection);

            World.resources.Add(new Resource(ResourceType.LARGE, 
                                    resource.transform.position,
                                    resource
                                ));
            resource.name += i.ToString();
        }

        for (int i = 0; i < sceneConfigurations.numberOfScoutRovers; i++)
        {
            Vector3 spawnDirection = GetSpawnDirection2(baseCampSpawnDirection, baseCampAngle, ignoreAngle);

            if (SharedInfo.isDeliberative) {
                DeliberativeRover rover = SpawnPrefabInSphereSurface(scoutDeliberativePrefab, spawnDirection).GetComponent<ScoutDeliberativeRover>();
                rover.name += i.ToString();
                World.deliberativeRovers.Add(rover);
            } else {
                Rover rover = SpawnPrefabInSphereSurface(scoutRoverPrefab, spawnDirection).GetComponent<ScoutRover>();
                rover.name += i.ToString();
                World.rovers.Add(rover);
            }
        }

        for (int i = 0; i < sceneConfigurations.numberOfCollectorRovers; i++)
        {
            Vector3 spawnDirection = GetSpawnDirection2(baseCampSpawnDirection, baseCampAngle, ignoreAngle);

            if (SharedInfo.isDeliberative) {
                DeliberativeRover rover = SpawnPrefabInSphereSurface(collectorDeliberativePrefab, spawnDirection).GetComponent<CollectorDeliberativeRover>();
                rover.name += i.ToString();
                World.deliberativeRovers.Add(rover);
            } else {
                Rover rover = SpawnPrefabInSphereSurface(collectorRoverPrefab, spawnDirection).GetComponent<CollectorRover>();
                rover.name += i.ToString();
                World.rovers.Add(rover);
            }
        }
    
    }

    /*
    * Instantiates a prefab in the planet surface  
    * positionInSphere - position in sphere surface  
    */
    private GameObject SpawnPrefabInSphereSurface(GameObject prefab, Vector3 positionInSphere) {

        Vector3 positionInSurface = (planet.transform.position + positionInSphere.normalized) 
        * (sceneConfigurations.planetRadius/2.0f + prefab.transform.localScale.y/2.0f);

        GameObject prefabInstance = Instantiate(prefab, positionInSurface, Quaternion.identity);
        prefabInstance.transform.localRotation = Quaternion.FromToRotation(prefab.transform.up, positionInSphere) * prefab.transform.rotation;
        prefabInstance.transform.parent = planet;

        return prefabInstance;
    }

    private Vector3 GetSpawnDirection(Vector3 ignoreDirection, float ignoreAngle) {
        Vector3 spawnDirection;

        do {
            spawnDirection = new Vector3(
                Random.Range(-1.0f,1.0f), 
                Random.Range(-1.0f,1.0f), 
                Random.Range(-1.0f,1.0f) 
            ).normalized;

        } while (Vector3.Angle(spawnDirection, ignoreDirection) <= ignoreAngle);

        return spawnDirection;
    }

    private Vector3 GetSpawnDirection2(Vector3 ignoreDirection, float startAngle, float endAngle) {
        Vector3 spawnDirection;

        float angle;

        do {
            spawnDirection = new Vector3(
                Random.Range(-1.0f,1.0f), 
                Random.Range(-1.0f,1.0f), 
                Random.Range(-1.0f,1.0f) 
            ).normalized;

            angle = Vector3.Angle(spawnDirection, ignoreDirection);


        } while (angle < startAngle || angle > endAngle);

        return spawnDirection;
    }
}
