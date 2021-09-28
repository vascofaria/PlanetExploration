using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    public SceneConfigurations[] configurations;
    private SceneConfigurations _selectedConfigurations;
    private bool _selectedAgentType = false; // false reactive, true deliberative
    private uint _selectedTimescale = 1;

    [SerializeField]
    private TextMeshProUGUI _timeScaleText;


    // Start is called before the first frame update
    void Start()
    {
        _selectedConfigurations = configurations[0];
    }


    public void OnWorldConfigChange(int value) {
        _selectedConfigurations = configurations[value];
    }

    public void OnAgentTypeChange(int value) {
        _selectedAgentType = value == 1;
    }

    public void OnTimescaleChange(float value) {
        _selectedTimescale = (uint)value;
        _timeScaleText.text = _selectedTimescale.ToString();
    }

    public void OnRun() {
        _selectedConfigurations.timeScale = _selectedTimescale;
        SharedInfo.configurations = _selectedConfigurations;
        SharedInfo.isDeliberative = _selectedAgentType;
        SharedInfo.changed = true;
        SceneManager.LoadScene(1);
    }

    public void OnQuit() {
        Application.Quit();
    }

}
