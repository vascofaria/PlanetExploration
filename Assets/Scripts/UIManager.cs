using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine.SceneManagement;
class UIManager : MonoBehaviour {

    public static UIManager uIManager;

    public TextMeshProUGUI elapsedTime;
    private Stopwatch stopWatch = new Stopwatch(); 
    public TextMeshProUGUI numberOfSmallResources; 
    public uint smallResources = 0;
    public TextMeshProUGUI numberOfMediumResources;
    public uint mediumResources = 0; 
    public TextMeshProUGUI numberOfLargeResources; 
    public uint largeResources = 0;
    public TextMeshProUGUI numberOfUselessTrips; 
    public uint uselessTrips = 0;
    public TextMeshProUGUI numberOfTripsToBase; 
    public uint tripsToBase = 0;
    public TextMeshProUGUI totalGasolineSpent;
    public float gasolineSpent = 0;
    public Button restartButton;
    public TextMeshProUGUI successMessage;
    public TextMeshProUGUI failedMessage;
    public SceneConfigurations sceneConfigurations;
    public TimeSpan timeSpan;

    private void Awake() {
        // We must set the uiManager on awake
        // to ensure it resets before every other script
        uIManager = this;
    }

    private void Start() {
        sceneConfigurations = SharedInfo.configurations;
        stopWatch.Start();
    } 
 
    private void Update() {
        timeSpan = new TimeSpan(stopWatch.Elapsed.Ticks * sceneConfigurations.timeScale);
 
        if (stopWatch.IsRunning) {
            elapsedTime.text = String.Format("{0:hh\\:mm\\:ss}", timeSpan);   
        }        

        numberOfSmallResources.text = smallResources.ToString();
        numberOfMediumResources.text = mediumResources.ToString();
        numberOfLargeResources.text = largeResources.ToString();
        numberOfTripsToBase.text = tripsToBase.ToString();
        numberOfUselessTrips.text = uselessTrips.ToString();
        totalGasolineSpent.text = ((int)gasolineSpent).ToString();
    }

    public void StopCountingTime() {
        stopWatch.Stop();
    }

    public void CountSmallResources(uint count) {
        smallResources += count;
    }

    public void CountMediumResources(uint count) {
        mediumResources += count;
    }

    public void CountLargeResources(uint count) {
        largeResources += count;
    }

    public void CountUselessTrips(uint count) {
        uselessTrips += count;
    }

    public void CountTripsToBase(uint count) {
        tripsToBase += count;
    }

    public void CountGasolineSpent(float count) {
        gasolineSpent += count;
    }

    public void ShowSuccessMessage() {
        successMessage.gameObject.SetActive(true);
    }

    public void ShowFailedMessage() {
        failedMessage.gameObject.SetActive(true);
    }

    public void OnMainMenuPress() {
        SceneManager.LoadScene(0);
    }

}