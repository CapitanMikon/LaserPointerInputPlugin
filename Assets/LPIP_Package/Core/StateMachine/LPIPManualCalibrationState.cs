using UnityEngine;
using System;

public class LPIPManualCalibrationState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;

    private bool isCalibrating;
    private bool firstClick ;

    private Pair restrictionTopLeft;
    private Pair restrictionBottomRight;
    
    private float factorX;
    private float factorY;

    private WindowData _windowData;
    private CameraData _cameraData;
    
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

    private int clickCounter;

    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("LPIP currentstate = {LPIPManualCalibrationState}");
        _lpipCoreManager = lpipCoreManager;
        Initialize();
        //try load saved calibration
        //else start anew
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
                switch (clickCounter)
                {
                    case 0:
                        real[0].x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                        real[0].y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                        break;
                    case 1:
                        real[1].x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                        real[1].y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                        break;
                    case 2:
                        real[2].x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                        real[2].y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                        break;
                    case 3:
                        real[3].x = Convert.ToInt32(screenPosition.x / _windowData.GAME_WINDOW_FACTORX);
                        real[3].y = Convert.ToInt32(screenPosition.y / _windowData.GAME_WINDOW_FACTORY);
                        Debug.Log("Calibrating ended.");
                        EndCalibration();
                        break;
                }

                clickCounter++;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                RestartCalibration();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                //CancelCalibration();
            }
        }
    }

    private void Initialize()
    {
        isCalibrating = false;
        firstClick = true;
        clickCounter = 0;
        _windowData = _lpipCoreManager.GetWindowData();
        _cameraData = _lpipCoreManager.GetCameraData();
        
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
        LPIPMouseEmulation.Instance.ShowCameraFeed();
        Debug.Log("Calibrating started.");
        Debug.Log("Please select 4 starting from points BL, BR, TR, TL.");
        isCalibrating = true;
    }

    private void EndCalibration()
    {
        LPIPMouseEmulation.Instance.HideCameraFeed();
        SaveCalibrationData();
        Debug.Log("Calibrating ended.");
        isCalibrating = false;
        _lpipCoreManager.SwitchState(_lpipCoreManager.RunningState);
    }

    private void RestartCalibration()
    {
        Debug.Log("Restarting calibration.");
        _lpipCoreManager.SwitchState(_lpipCoreManager.ManualCalibrationState);
    }
    
    private void CancelCalibration()
    {
        LPIPMouseEmulation.Instance.HideCameraFeed();
        Debug.Log("Cancelling calibration.");
        _lpipCoreManager.SwitchState(_lpipCoreManager.InitializationState);
    }

    private void SaveCalibrationData()
    {
        LPIPCalibrationData _calibrationData = new LPIPCalibrationData
        {
            restrictionTopLeft = restrictionTopLeft,
            restrictionBottomRight = restrictionBottomRight,
            factorX = factorX,
            factorY = factorY,
            real = real,
            ideal = ideal,
        };

        _lpipCoreManager.SetCalibrationData(_calibrationData);
    }
}
