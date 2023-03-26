using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public class LaserPointerInputManager : MonoBehaviour
{
    private LaserPointerInputBaseState currentState;

    public LaserPointerInputSetUp setUpState = new LaserPointerInputSetUp();
    public LaserPointerInputCalibration calibrationState = new LaserPointerInputCalibration();
    public LaserPointerInputInOperation inOperationState = new LaserPointerInputInOperation();
    public LaserPointerInputOffline offlineState = new LaserPointerInputOffline();

    private CallibrationData _callibrationData;
    private CameraData _cameraData;
    private WindowData _windowData;
    
    [SerializeField] public RawImage webcamImageHolder;

    [SerializeField] private UnityEvent OnCalibrationDone; 
    [SerializeField] private UnityEvent OnLaserInputRegistered; 
    
    public WebCamTexture webCamTexture;

    public int PROJECTOR_DISPLAY_ID = 1; // ask user what screen is projector, usually 2nd aside from 1st main screen
    
    private Vector3 markerDefaultPosition = new Vector3(-100, -100, 0);

    [SerializeField] private GameObject markerSprite;
    
    void Start()
    {
        ResetMarkerSpritePosition();
        currentState = offlineState;
        currentState.EnterState(this);
    }
    
    void Update()
    {
        currentState.UpdateState();
    }

    public void SwitchState(LaserPointerInputBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public void InvokeOnLaserPointerInputDetectedEvent()
    {
        OnLaserInputRegistered?.Invoke();
    }
    
    public void InvokeOnLaserPointerCalibrationEndedEvent()
    {
        OnCalibrationDone?.Invoke();
    }

    public void UpdateMarkerSpritePosition(float x, float y)
    {
        markerSprite.transform.position = new Vector3(x, y, 0);
    }
    
    public void ResetMarkerSpritePosition()
    {
        Debug.LogWarning("Marker was reset off screen!");
        markerSprite.transform.position = markerDefaultPosition;
    }

    public void SetCalibrationData(CallibrationData data)
    {
        _callibrationData = data;
    }

    public CallibrationData GetCallibrationData()
    {
        return _callibrationData;
    }
    
    public void SetCameraData(CameraData data)
    {
        _cameraData = data;
    }

    public CameraData GetCameraData()
    {
        return _cameraData;
    }
    
    public void SetWindowData(WindowData data)
    {
        _windowData = data;
    }

    public WindowData GetWindowData()
    {
        return _windowData;
    }
}
