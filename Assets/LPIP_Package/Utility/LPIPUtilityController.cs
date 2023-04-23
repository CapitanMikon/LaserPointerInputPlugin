using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class LPIPUtilityController : MonoBehaviour
{
    [Header("Configuration Menu")]
    [SerializeField] private LPIPConfigurationMenuController lpipConfigurationMenuController;
    [SerializeField] private GameObject ConfigurationMenuContent;
    
    [Header("Calibration Menu")]
    [SerializeField] private LPIPCalibrationMenuController lpipCalibrationMenuController;
    [SerializeField] private GameObject CalibrationMenuContent;
    
    [Header("Debug Menu")]
    [SerializeField] private GameObject DebugTextContent;
    
    [Header("Marker")]
    [SerializeField] private GameObject MarkerContent;
    
    [Header("Camera stuff")]
    [SerializeField] private GameObject cameraFeed;
    [SerializeField] private RawImage cameraFeedImage;
    private WebCamTexture _webCamTexture;
    
    //private static LPIPUtilityController Instance;

    public static event Action OnUtilityMenuEnabled;
    public static event Action OnUtilityMenuDisabled;

    private void Awake()
    {
        /*if (Instance == null)
        {
            Instance = this;
        }*/
    }

    private void Start()
    {
        CloseUtilityUI();
    }

    private void OnEnable()
    {
        LPIPCoreManager.OnCalibrationStartedEvent += ShowCameraFeedEventHandler;
        LPIPCoreManager.OnCalibrationFinishedEvent += HideCameraFeedEventHandler;
    }

    private void OnDisable()
    {
        LPIPCoreManager.OnCalibrationStartedEvent -= ShowCameraFeedEventHandler;
        LPIPCoreManager.OnCalibrationFinishedEvent -= HideCameraFeedEventHandler;
    }
    

    public void CalibratePlugin()
    {
        LPIPCoreController.Instance.ReCalibrateLPIP();
    }

    public void ConfigurationSetupSave()
    {
        DebugTextController.Instance.ResetText(DebugTextController.DebugTextGroup.Everything);
        LPIPCoreController.Instance.ResetLPIP();
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }
        
        //get values from dropdown menu and init plugin core
        var projectorId = lpipConfigurationMenuController.GetProjectorIdFromDropdown();
        var cameraDevice = lpipConfigurationMenuController.GetWebCamDeviceFromDropdown();
        
        _webCamTexture = new WebCamTexture(cameraDevice.name);
        cameraFeedImage.texture = _webCamTexture;
        _webCamTexture.Play();
        
        LPIPCoreController.Instance.SetProjectorDisplayId(projectorId);
        LPIPCoreController.Instance.SetWebCamTexture(_webCamTexture);
        
        LPIPCoreController.Instance.InitializeLPIP();
        
        lpipCalibrationMenuController.EnableCalibButton();
        ConfigurationMenuContent.SetActive(false);
        CalibrationMenuContent.SetActive(true);
    }

    public void ConfigurationSetupCancel()
    {
        ConfigurationMenuContent.SetActive(false);
        CalibrationMenuContent.SetActive(true);
    }

    public void ConfigurationSetupEnter()
    {
        ConfigurationMenuContent.SetActive(true);
        CalibrationMenuContent.SetActive(false);
    }

    public void CloseUtilityUI()
    {
        ConfigurationMenuContent.SetActive(false);
        CalibrationMenuContent.SetActive(false);
        OnUtilityMenuDisabled?.Invoke();
    }
    
    public void OpenUtilityUI()
    {
        ConfigurationMenuContent.SetActive(false);
        CalibrationMenuContent.SetActive(true);
        OnUtilityMenuEnabled?.Invoke();
    }

    public void EnableMarker()
    {
        Debug.Log("Marker enabled");
        MarkerContent.SetActive(true);
    }

    public void DisableMarker()
    {
        Debug.Log("Marker disabled");
        MarkerContent.SetActive(false);
    }

    public void EnableDebugText()
    {
        Debug.Log("DebugText enabled");
        DebugTextContent.SetActive(true);
    }

    public void DisableDebugText()
    {
        Debug.Log("DebugText disabled");
        DebugTextContent.SetActive(false);
    }
    private void HideCameraFeedEventHandler(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        Debug.Log("HideCameraFeedEvent received!");
        SetActiveCameraFeed(false);
    }
    
    private void ShowCameraFeedEventHandler()
    {
        Debug.Log("ShowCameraFeedEvent received!");
        SetActiveCameraFeed(true);
    }

    public void SetActiveCameraFeed(bool isActive)
    {
        cameraFeed.SetActive(isActive);
    }
}
