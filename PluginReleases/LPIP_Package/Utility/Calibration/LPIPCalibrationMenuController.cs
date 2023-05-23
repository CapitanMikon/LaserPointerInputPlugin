using System;
using UnityEngine;
using UnityEngine.UI;

public class LPIPCalibrationMenuController : MonoBehaviour
{
    
    [SerializeField] private LPIPUtilityController lpipUtilityController;

    [SerializeField] private Button editConfigurationButton;
    [SerializeField] private Button calibrateButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private Toggle markerToggle;
    [SerializeField] private Toggle debugTextToggle;


    private void Start()
    {
        markerToggle.isOn = false;
        debugTextToggle.isOn = false;
    }

    private void OnEnable()
    {
        exitButton.interactable = true;
        calibrateButton.interactable = false;
        LPIPCoreManager.OnCalibrationFinishedEvent += CalibrationFinishedHandler;
    }
    
    private void OnDisable()
    {
        LPIPCoreManager.OnCalibrationFinishedEvent -= CalibrationFinishedHandler;
    }

    public void EnableCalibButton()
    {
        calibrateButton.interactable = true;
    }

    public void OnEditConfigurationClick()
    {
        lpipUtilityController.ConfigurationSetupEnter();
    }
    
    public void OnCalibrateClick()
    {
        editConfigurationButton.interactable = false;
        calibrateButton.interactable = false;
        lpipUtilityController.CalibratePlugin();
    }

    public void OnExitButtonClick()
    {
        lpipUtilityController.CloseUtilityUI();
    }

    public void OnValueChangedMarkerToggle(bool newValue)
    {
        if (newValue)
        {
            lpipUtilityController.EnableMarker();
        }
        else
        {
            lpipUtilityController.DisableMarker();
        }
    }
    
    public void OnValueChangedDebugTextToggle(bool newValue)
    {
        if (newValue)
        {
            lpipUtilityController.EnableDebugText();
        }
        else
        {
            lpipUtilityController.DisableDebugText();
        }
    }

    private void CalibrationFinishedHandler(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        if (result == LPIPManualCalibrationState.LPIPCalibrationResult.Normal)
        {
            editConfigurationButton.interactable = true;
            calibrateButton.interactable = true;
        }
    }
}
