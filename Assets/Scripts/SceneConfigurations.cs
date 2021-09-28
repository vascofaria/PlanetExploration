using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scene Configurations")]
public class SceneConfigurations : ScriptableObject
{
    [Range(30.0f, 1000.0f)]
    public float planetRadius;

    public uint numberOfSmallResources;
    public uint numberOfMediumResources;
    public uint numberOfLargeResources;

    public uint numberOfScoutRovers;

    public uint numberOfCollectorRovers;

    [Range(1, 100)]
    public uint timeScale;
}