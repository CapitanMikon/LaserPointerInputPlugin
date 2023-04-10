using System;
using UnityEngine;

public class LPIPCoreManager : MonoBehaviour
{
    
    public LPIPBaseState InitializationState { get; private set; }
    public LPIPBaseState ManualCalibrationState {get; private set;}
    //public LPIPBaseState AutomaticCalibrationState {get; private set;}
    public LPIPBaseState RunningState { get; private set; }
    public LPIPBaseState StandbyState { get; private set; }

    private LPIPBaseState _currentState;

    public LPIPCalibrationData LpipCalibrationData  { get;  set; }
    public CameraData CameraData  { get;  set; }
    public WindowData WindowData  { get;  set; }
    public WebCamTexture WebCamTexture { get; set; }
    [HideInInspector] public int PROJECTOR_DISPLAY_ID = 1; // ask user what screen is projector, usually 2nd aside from 1st main screen

    public static event Action OnCalibrationStartedEvent;
    public static event Action<LPIPManualCalibrationState.LPIPCalibrationResult> OnCalibrationFinishedEvent;
    public static event Action OnDetectionStartedEvent;
    public static event Action OnDetectionStoppedEvent;
    public static event Action<Vector2> OnLaserHitDownDetectedEvent;
    public static event Action<Vector2> OnLaserHitUpDetectedEvent;


    private void Awake()
    {
        InitializationState = new LPIPInitializationState();
        ManualCalibrationState = new LPIPManualCalibrationState();
        //AutomaticCalibrationState = new LPIPAutomaticCalibrationState();
        RunningState = new LPIPRunningState();
        StandbyState = new LPIPStandbyState();
    }

    private void Start()
    {
        SwitchState(StandbyState);
        //_currentState = StandbyState;
        _currentState.EnterState(this);
    }
    
    private void Update()
    {
        _currentState.UpdateState();
    }

    private bool TransitionToStateIsAllowed(LPIPBaseState state)
    {
        if (_currentState == null)
        {
            return state == StandbyState;
        }
        if (_currentState == InitializationState)
        {
            return state == ManualCalibrationState || state == InitializationState;// || state == AutomaticCalibrationState;
        }
        if (_currentState == ManualCalibrationState)// || state == AutomaticCalibrationState)
        {
            return state == InitializationState || state == ManualCalibrationState || state == RunningState;// || state == AutomaticCalibrationSate;
        }
        if (_currentState == RunningState)
        {
            return state == ManualCalibrationState || state == StandbyState;// || state == AutomaticCalibrationState;
        }
        if (_currentState == StandbyState)
        {
            return state == InitializationState || state == StandbyState;
        }

        return false;
    }

    public void SwitchState(LPIPBaseState state)
    {
        if (TransitionToStateIsAllowed(state))
        {
            Debug.Log($"Changing state from <color=#c1a730>{_currentState}</color> to <color=#36ba1f>{state}</color>");
            _currentState?.ExitState();
            _currentState = state;
            _currentState?.EnterState(this);
        }
        else
        {
            Debug.LogError($"State transition from [{_currentState}] to [{state}] is not allowed!");
        }
    }

    public void InvokeOnLaserHitDownDetectedEvent(Vector2 clickPosition)
    {
        Debug.LogWarning($"Fired event OnLaserHitDownDetectedEvent = {clickPosition}");
        OnLaserHitDownDetectedEvent?.Invoke(clickPosition);
    }
    
    public void InvokeOnLaserHitUpDetectedEvent(Vector2 clickPosition)
    {
        Debug.LogWarning($"Fired event OnLaserHitUpDetectedEvent = {clickPosition}");
        OnLaserHitUpDetectedEvent?.Invoke(clickPosition);
    }
    
    public void InvokeCalibrationEndedEvent(LPIPManualCalibrationState.LPIPCalibrationResult result)
    {
        Debug.LogWarning($"Fired event OnCalibrationFinishedEvent = {result}");
        OnCalibrationFinishedEvent?.Invoke(result);
    }
    
    public void InvokeCalibrationStartedEvent()
    {
        Debug.LogWarning("Fired event OnCalibrationStartedEvent");

        OnCalibrationStartedEvent?.Invoke();
    }
    
    public void InvokeDetectionStartedEvent()
    {
        Debug.LogWarning("Fired event OnDetectionStartedEvent");

        OnDetectionStartedEvent?.Invoke();
    }

    public void InvokeDetectionStoppedEvent()
    {
        Debug.LogWarning("Fired event OnDetectionStoppedEvent");
        
        OnDetectionStoppedEvent?.Invoke();
    }

    public void StartLPIP()
    {
        Debug.LogWarning("LPIP Start requested!");
        SwitchState(InitializationState);
    }
    
    public void ResetLPIP()
    {
        Debug.LogWarning("LPIP Reset requested!");
        SwitchState(StandbyState);
    }
    
    public void ReCalibrateLPIP()
    {
        Debug.LogWarning("LPIP Recalibration requested!");
        SwitchState(ManualCalibrationState);
    }
}
