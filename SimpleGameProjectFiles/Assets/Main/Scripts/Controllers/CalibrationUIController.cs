using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationUIController : MonoBehaviour
{
    private UIController _uiController;

    private bool firstCalibration = true;

    [SerializeField] private Button startPluginButton;
    [SerializeField] private Button mainMenuButton;
    
    [SerializeField] private Button calibrationButton;
    [SerializeField] private TMP_Text calibrationButtonText;

    private const string calibButtonTextStart = "Start calibration";
    private const string calibButtonTextRestart = "calibration in progress";
    private const string calibButtonTextRecalibrate = "Recalibrate";

    private CalibrationType _calibrationType;
    private void Start()
    {
        _calibrationType = CalibrationType.initial;
        UpdateCalibrationButtonText();
        SetButtonInteractable(CalibrationButtonType.start, true);
        SetButtonInteractable(CalibrationButtonType.calibrate, false);
        
        startPluginButton.onClick.AddListener(OnStartPluginButton);
        calibrationButton.onClick.AddListener(OnCalibrationButton);
    }

    private void OnEnable()
    {
        //LPIPCoreManager.OnCalibrationFinishedEvent += CalibrationFinishedEventHandler;
    }

    private void OnDisable()
    {
        //LPIPCoreManager.OnCalibrationFinishedEvent -= CalibrationFinishedEventHandler;
    }

    public void SetController(UIController uiController)
    {
        _uiController = uiController;
    }
    
    public void OnMainMenuButton() //assigned in inspector
    {
        SetButtonInteractable(CalibrationButtonType.start, true);
        _uiController.OnMainMenuButton();
        UpdateCalibrationButtonText();
    }

    private void OnCalibrationButton()
    {
        ReCalibrate();
    }
    private void OnStartPluginButton()
    {
        StartPlugin();
        SetButtonInteractable(CalibrationButtonType.start, false);
    }

    /*private void CalibrationFinishedEventHandler(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        Debug.LogWarning($"OnCalibrationFinishedEvent received = {result}!");
        if (result == LPIPManualCalibrationState.LPIPCalibrationResult.Normal)
        {
            SetButtonInteractable(CalibrationButtonType.calibrate, true);
            _calibrationType = CalibrationType.done;
            UpdateCalibrationButtonText();
            mainMenuButton.interactable = true;
        }else if (result == LPIPManualCalibrationState.LPIPCalibrationResult.Restart)
        {
            SetButtonInteractable(CalibrationButtonType.calibrate, true);
            _calibrationType = CalibrationType.inprogress;
            UpdateCalibrationButtonText();
        }
    }*/
    
    private void SetButtonInteractable(CalibrationButtonType buttonType, bool isInteractable)
    {
        switch (buttonType)
        {
            case CalibrationButtonType.start:
                startPluginButton.interactable = isInteractable;
                break;
            case CalibrationButtonType.calibrate:
                calibrationButton.interactable = isInteractable;
                break;
        } 
        Debug.Log($"Setting button {buttonType} to {isInteractable}");
    }

    private void UpdateCalibrationButtonText()
    {
        if (_calibrationType == CalibrationType.initial)
        {
            calibrationButtonText.text = calibButtonTextStart;
        }else if (_calibrationType == CalibrationType.inprogress)
        {
            calibrationButtonText.text = calibButtonTextRestart;
            
        }else if (_calibrationType == CalibrationType.done)
        {
            calibrationButtonText.text = calibButtonTextRecalibrate;
        }
    }

    private void ReCalibrate()
    {
        //LPIPCoreController.Instance.RecalibrateLPIP();
    }
    private void StartPlugin()
    {
        //LPIPCoreController.Instance.ActivateLPIP();
        _calibrationType = CalibrationType.inprogress;
        UpdateCalibrationButtonText();
        SetButtonInteractable(CalibrationButtonType.calibrate, false);
        if (firstCalibration)
        {
            mainMenuButton.interactable = false;
            firstCalibration = false;
        }
    }

    enum CalibrationType
    {
        initial,
        inprogress,
        done
    }
    enum CalibrationButtonType
    {
        start,
        calibrate
    }
}
