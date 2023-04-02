using System;
using UnityEngine;
using UnityEngine.UI;

public class LPIPCalibrationUIController : MonoBehaviour
{
    [SerializeField] private LPIPConfigurationMenuController lpipConfigurationMenuController;
    [SerializeField] private LPIPCalibrationHelperController lpipCalibrationHelperController;
    [SerializeField] private GameObject cameraFeed;
    [SerializeField] private RawImage cameraFeedImage;
    private WebCamTexture _webCamTexture;

    [SerializeField] private GameObject Menu1Content;
    [SerializeField] private GameObject Menu2Content;
    [SerializeField] private GameObject DebugUIContent;

    public static LPIPCalibrationUIController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ShowUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowUI();
        }
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

    public void ConfigurationSetupLeave()
    {
        //get values from dropdown menu and init plugin core
        var projectorId = lpipConfigurationMenuController.GetProjectorIdFromDropdown();
        var cameraDevice = lpipConfigurationMenuController.GetWebCamDeviceFromDropdown();
        
        _webCamTexture = new WebCamTexture(cameraDevice.name);
        cameraFeedImage.texture = _webCamTexture;
        _webCamTexture.Play();
        
        LPIPCoreController.Instance.SetProjectorDisplayId(projectorId);
        LPIPCoreController.Instance.SetWebCamTexture(_webCamTexture);
        
        LPIPCoreController.Instance.InitializeLPIP();

        //hide menu1
        lpipCalibrationHelperController.EnableCalibButton();
        Menu1Content.SetActive(false);
        //show menu2
        Menu2Content.SetActive(true);
        
    }

    public void ConfigurationSetupEnter()
    {
        LPIPCoreController.Instance.ResetLPIP();
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }
        Menu1Content.SetActive(true);
        Menu2Content.SetActive(false);
    }

    public void HideUI()
    {
        Menu1Content.SetActive(false);
        Menu2Content.SetActive(false);
        DebugUIContent.SetActive(false);
    }
    
    public void ShowUI()
    {
        Menu1Content.SetActive(false);
        Menu2Content.SetActive(true);
        DebugUIContent.SetActive(true);
    }
    private void HideCameraFeedEventHandler(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        Debug.Log("HideCameraFeedEvent received!");
        HideCameraFeed();
    }
    
    private void ShowCameraFeedEventHandler()
    {
        Debug.Log("ShowCameraFeedEvent received!");
        ShowCameraFeed();
    }

    public void ShowCameraFeed()
    {
        Debug.Log("CameraFeed ON");
        cameraFeed.SetActive(true);
    }
    
    public void HideCameraFeed()
    {
        Debug.Log("CameraFeed OFF");
        cameraFeed.SetActive(false);
    }
}
