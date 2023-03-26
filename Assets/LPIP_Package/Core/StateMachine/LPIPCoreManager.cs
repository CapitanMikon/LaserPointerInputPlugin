using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public class LPIPCoreManager : MonoBehaviour
{
    public LPIPBaseState InitializationState { get; private set; }
    public LPIPBaseState ManualCalibrationState {get; private set;}
    //public LPIPBaseState AutomaticCalibrationState {get; private set;}
    public LPIPBaseState RunningState { get; private set; }
    public LPIPBaseState StandbyState { get; private set; }

    
    private LPIPBaseState _currentState;

    private LPIPCalibrationData _lpipCalibrationData;
    private CameraData _cameraData;
    private WindowData _windowData;
    
    [SerializeField] public RawImage webcamImageHolder;

    [SerializeField] private UnityEvent OnCalibrationDone; 
    [SerializeField] private UnityEvent OnLaserInputRegistered; 
    
    public WebCamTexture webCamTexture;

    public int PROJECTOR_DISPLAY_ID = 1; // ask user what screen is projector, usually 2nd aside from 1st main screen
    
    private readonly Vector3 _laserPointerMarketDefaultPosition = new Vector3(-100, -100, 0);
    [SerializeField] private GameObject markerSprite;

    private void Awake()
    {
        InitializationState = new LPIPInitializationState();
        ManualCalibrationState = new LPIPManualCalibrationState();
        //AutomaticCalibrationState = new LPIPAutomaticCalibrationState();
        RunningState = new LPIPRunningState();
        StandbyState = new LPIPStandbyState();
    }

    void Start()
    {
        ResetLaserMarkerPos();
        _currentState = StandbyState;
        _currentState.EnterState(this);
    }
    
    void Update()
    {
        _currentState.UpdateState();
    }

    public void SwitchState(LPIPBaseState state)
    {
        _currentState.ExitState();
        _currentState = state;
        _currentState.EnterState(this);
    }

    public void InvokeOnLaserPointerInputDetectedEvent()
    {
        OnLaserInputRegistered?.Invoke();
    }
    
    public void InvokeOnLaserPointerCalibrationEndedEvent()
    {
        OnCalibrationDone?.Invoke();
    }

    public void UpdateLaserMarkerPos(float x, float y)
    {
        markerSprite.transform.position = new Vector3(x, y, 0);
    }
    
    public void ResetLaserMarkerPos()
    {
        Debug.LogWarning("Marker was reset off screen!");
        markerSprite.transform.position = _laserPointerMarketDefaultPosition;
    }

    public void SetCalibrationData(LPIPCalibrationData data)
    {
        _lpipCalibrationData = data;
    }

    //
    //calibration data methods
    //
    public LPIPCalibrationData GetCalibrationData()
    {
        return _lpipCalibrationData;
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
