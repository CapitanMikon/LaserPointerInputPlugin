using UnityEngine;
using System;

public class LPIPManualCalibrationState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;

    private bool isCalibrating;
    private int clickCounter;

    private WindowData _windowData;
    private CameraData _cameraData;
    
    
    private bool firstClick;
    Vector2 restrictionTopLeft = Vector2.zero;
    Vector2 restrictionBottomRight = Vector2.zero;
    
    private float factorX;
    private float factorY;

    private Vector2[] real = new Vector2[4]{
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0)
    };

    // The four corners of the destination image
    private Vector2[] ideal = new Vector2[4]{
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0)
    };


    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        firstClick = true;
        Debug.Log("Entered state {LPIPManualCalibrationState}");
        _lpipCoreManager = lpipCoreManager;
        
        Initialize();

        _cameraData = _lpipCoreManager.CameraData;
        
        //try load saved calibration
        //or start anew
        
        StartCalibration();
    }

    public override void UpdateState()
    {
        if (isCalibrating)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Debug.Log($"Mouse click at [{screenPosition.x}, {screenPosition.y}]");
                if (firstClick)
                {
                    restrictionTopLeft.x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                    restrictionTopLeft.y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                    firstClick = false;
                }
                else
                {
                    restrictionBottomRight.x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                    restrictionBottomRight.y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                    isCalibrating = false;
                    //calculate factors of camera resolution and selected area resolution
                    var restrictedZoneHeight = Mathf.Abs(restrictionTopLeft.y - restrictionBottomRight.y);
                    var restrictedZoneWidth = Mathf.Abs(restrictionTopLeft.x - restrictionBottomRight.x);
                    factorY = Convert.ToSingle(_cameraData.CAMERA_HEIGHT) / restrictedZoneHeight;
                    factorX = Convert.ToSingle(_cameraData.CAMERA_WIDTH) / restrictedZoneWidth;

                    Debug.LogWarning($"Scaling factor is {factorX}:{factorY}");
                    EndCalibration();
                }
            }
        }
    }
    
    public override void ExitState()
    {
        if (isCalibrating)
        {
            _lpipCoreManager.InvokeCalibrationEndedEvent(LPIPCalibrationResult.Restart);
        }
        Debug.Log("Leaving state {LPIPManualCalibrationState}");
    }

    private void Initialize()
    {
        isCalibrating = false;
        clickCounter = 0;
        _windowData = _lpipCoreManager.WindowData;
        
        ideal[0].x = 0;
        ideal[0].y = 0;
        
        ideal[1].x = _windowData.GAME_WINDOW_WIDTH;
        ideal[1].y = 0;
        
        ideal[2].x = _windowData.GAME_WINDOW_WIDTH;
        ideal[2].y = _windowData.GAME_WINDOW_HEIGHT;
        
        ideal[3].x = 0;
        ideal[3].y = _windowData.GAME_WINDOW_HEIGHT;
        

    }

    private void StartCalibration()
    {
        _lpipCoreManager.InvokeCalibrationStartedEvent();
        Debug.Log("Calibrating started.");
        Debug.Log("Please select 4 starting from points BL, BR, TR, TL.");
        isCalibrating = true;
    }

    private void EndCalibration()
    {
        _lpipCoreManager.InvokeCalibrationEndedEvent(LPIPCalibrationResult.Normal);
        SaveCalibrationData();
        Debug.Log("Calibrating ended. Fired event!");
        isCalibrating = false;
        _lpipCoreManager.SwitchState(_lpipCoreManager.RunningState);
    }

    private void SaveCalibrationData()
    {
        var calibrationData = new LPIPCalibrationData
        {
            real = real,
            ideal = ideal,
            factorX = factorX,
            factorY = factorY,
            restrictionBottomRight = restrictionBottomRight,
            restrictionTopLeft = restrictionTopLeft,
        };

        _lpipCoreManager.LpipCalibrationData = calibrationData;
    }
    
    public enum LPIPCalibrationResult
    {
        Normal,
        Restart,
        Cancel,
        Error,
    }
}

