using UnityEngine;
using System;

public class LaserPointerInputCalibration : LaserPointerInputBaseState
{
    private LaserPointerInputManager _laserPointerInputManager;

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

    public override void EnterState(LaserPointerInputManager laserPointerInputManager)
    {
        Debug.Log("STATE: Calibration");
        _laserPointerInputManager = laserPointerInputManager;
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
        _windowData = _laserPointerInputManager.GetWindowData();
        _cameraData = _laserPointerInputManager.GetCameraData();
        
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
        VirtualMouse.instance.ShowCameraFeed();
        Debug.Log("Calibrating started.");
        Debug.Log("Please select 4 starting from points BL, BR, TR, TL.");
        isCalibrating = true;
    }

    private void EndCalibration()
    {
        VirtualMouse.instance.HideCameraFeed();
        SaveCalibrationData();
        Debug.Log("Calibrating ended.");
        isCalibrating = false;
        _laserPointerInputManager.SwitchState(_laserPointerInputManager.inOperationState);
    }

    private void RestartCalibration()
    {
        Debug.Log("Restarting calibration.");
        _laserPointerInputManager.SwitchState(_laserPointerInputManager.calibrationState);
    }
    
    private void CancelCalibration()
    {
        VirtualMouse.instance.HideCameraFeed();
        Debug.Log("Cancelling calibration.");
        _laserPointerInputManager.SwitchState(_laserPointerInputManager.setUpState);
    }

    private void SaveCalibrationData()
    {
        CallibrationData _calibrationData = new CallibrationData
        {
            restrictionTopLeft = restrictionTopLeft,
            restrictionBottomRight = restrictionBottomRight,
            factorX = factorX,
            factorY = factorY,
            real = real,
            ideal = ideal,
        };

        _laserPointerInputManager.SetCalibrationData(_calibrationData);
    }
}
