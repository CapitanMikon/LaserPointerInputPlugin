using UnityEngine;

public class LPIPCalibrationUIController : MonoBehaviour
{
    
    [SerializeField] private GameObject cameraImageFeed;

    public static LPIPCalibrationUIController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        cameraImageFeed.SetActive(true);
    }
    
    public void HideCameraFeed()
    {
        Debug.Log("CameraFeed OFF");
        cameraImageFeed.SetActive(false);
    }
}
