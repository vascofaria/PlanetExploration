using UnityEngine;
using System.Collections.Generic;
using System.IO;

class MainManager : MonoBehaviour {

    public SceneConfigurations sceneConfigurations;
    public WorldGenerator worldGenerator;
    public UIManager uIManager;

    public bool isDeliberative = false;

    private StreamWriter _streamSmall;
    private StreamWriter _streamMedium;
    private StreamWriter _streamLarge;

    private StreamWriter _times;

    public static int name = 0;

    private void Awake() {
        // World must be reset on awake otherwise its static fields will save the information
        // from the previous run
        World.Reset();

        if (!SharedInfo.changed) {
            SharedInfo.isDeliberative = isDeliberative;
            SharedInfo.configurations = sceneConfigurations;
        } else {
            isDeliberative = SharedInfo.isDeliberative;
        }

        //ClearMetricFiles();

        _times = new StreamWriter("Assets/Metrics/times.csv", true);

        _streamSmall = new StreamWriter("Assets/Metrics/Small" + name.ToString() + ".csv", true);
        _streamMedium = new StreamWriter("Assets/Metrics/Medium" + name.ToString() + ".csv", true);
        _streamLarge = new StreamWriter("Assets/Metrics/Large" + name.ToString() + ".csv", true);

        ++name;
    }
    
    void Start() {
        sceneConfigurations = SharedInfo.configurations;

        int ignoreCollisionLayer = LayerMask.NameToLayer("Ignore Collisions");
        
        Physics.IgnoreLayerCollision(ignoreCollisionLayer, ignoreCollisionLayer, true);

        Time.timeScale = sceneConfigurations.timeScale;
    }

    void Update() {
        if (CheckSuccess()) {
            ForceWriteMetrics();
            CloseStreams();
            WriteAllMetrics();
            Time.timeScale = 0.0f;
            uIManager.StopCountingTime();
            uIManager.ShowSuccessMessage();
        } else if (CheckFail()) {
            Time.timeScale = 0.0f;
            uIManager.StopCountingTime();
            uIManager.ShowFailedMessage();
        } else {
            WriteMetrics();
        }
    }

    private bool CheckSuccess() {
        return uIManager.smallResources == sceneConfigurations.numberOfSmallResources &&
            uIManager.mediumResources == sceneConfigurations.numberOfMediumResources && 
            uIManager.largeResources == sceneConfigurations.numberOfLargeResources;
    }

    private bool CheckFail() {

        int brokenScouts = 0;

        if (SharedInfo.isDeliberative) {
            foreach(DeliberativeRover rover in World.deliberativeRovers) {
                if (rover is ScoutDeliberativeRover && rover.IsBroken()) {
                    ++brokenScouts;
                }
            }
        } else {
            foreach(Rover rover in World.rovers) {
                if (rover is ScoutRover && rover.IsBroken()) {
                    ++brokenScouts;
                }
            }
        } 

        return brokenScouts == sceneConfigurations.numberOfScoutRovers;
    }

    private float currTime = 0;
    private float writeInterval = 0.5f;
    private float errorMargin = 0.1f;

    private float printValue = 0.0f;
    private int numberOfPrints = 0;
    private void WriteMetrics() {

        currTime += Time.deltaTime;

        int nbrOfPrints = (int) (currTime/writeInterval);

        if (nbrOfPrints > numberOfPrints) {
            int diff = nbrOfPrints - numberOfPrints;
            numberOfPrints += diff;
            for (int i = 0; i < diff; i++)
            {
                _streamSmall.WriteLine(printValue.ToString().Replace(',','.')  + "," + uIManager.smallResources);
                _streamMedium.WriteLine(printValue.ToString().Replace(',','.')  + "," + uIManager.mediumResources);
                _streamLarge.WriteLine(printValue.ToString().Replace(',','.') + "," + uIManager.largeResources);
                printValue += writeInterval;
            }
        }
    }

    private void ForceWriteMetrics() {
        if (_streamSmall != null && _streamMedium != null && _streamLarge != null) {
            _streamSmall.WriteLine(printValue.ToString().Replace(',','.')  + "," + uIManager.smallResources);
            _streamMedium.WriteLine(printValue.ToString().Replace(',','.')  + "," + uIManager.mediumResources);
            _streamLarge.WriteLine(printValue.ToString().Replace(',','.') + "," + uIManager.largeResources);
        }
    }

    private void WriteAllMetrics() {
        if (_times != null) {
            _times.WriteLine(uIManager.timeSpan.TotalSeconds.ToString().Replace(',','.')  + "," + uIManager.uselessTrips + "," + uIManager.tripsToBase
             + "," + uIManager.gasolineSpent.ToString().Replace(',','.'));
            _times.Close();
            _times = null;
        }
    }

    private void ClearMetricFiles() {
        File.WriteAllText("Assets/Metrics/Small.csv",string.Empty);
        File.WriteAllText("Assets/Metrics/Medium.csv",string.Empty);
        File.WriteAllText("Assets/Metrics/Large.csv",string.Empty);
    }
    private void OnDestroy() {
        CloseStreams();
    }

    private void CloseStreams() {
        if (_streamSmall != null && _streamMedium != null && _streamLarge != null) {
            _streamSmall.Close();
            _streamMedium.Close();
            _streamLarge.Close();

            _streamSmall.Dispose();
            _streamSmall = null;

            _streamMedium.Dispose();
            _streamSmall = null;

            _streamLarge.Dispose();
            _streamLarge = null;
        }
    }
}