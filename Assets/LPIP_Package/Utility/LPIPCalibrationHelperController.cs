using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LPIPCalibrationHelperController : MonoBehaviour
{
    
    [SerializeField] private LPIPCalibrationUIController lpipCalibrationUIController;

    [SerializeField] private Button editConfigurationButton;
    [SerializeField] private Button calibrateButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        exitButton.interactable = false;
        LPIPCoreManager.OnCalibrationFinishedEvent += CalibrationFinishedHandler;
    }
    
    private void OnDisable()
    {
        LPIPCoreManager.OnCalibrationFinishedEvent -= CalibrationFinishedHandler;
    }

    public void OnEditConfigurationClick()
    {
        lpipCalibrationUIController.ConfigurationSetupEnter();
    }
    
    public void OnCalibrateClick()
    {
        editConfigurationButton.interactable = false;
        calibrateButton.interactable = false;
        lpipCalibrationUIController.CalibratePlugin();
    }

    public void OnExitButtonClick()
    {
        lpipCalibrationUIController.HideUI();
    }

    private void CalibrationFinishedHandler(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        if (result == LPIPManualCalibrationState.LPIPCalibrationResult.Normal)
        {
            editConfigurationButton.interactable = true;
            calibrateButton.interactable = true;
            exitButton.interactable = true;
        }
    }
}
