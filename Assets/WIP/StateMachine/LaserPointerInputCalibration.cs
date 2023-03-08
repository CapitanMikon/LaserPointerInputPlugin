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
                    _laserPointerInputManager.InvokeOnLaserPointerCalibrationEndedEvent();
                    //calculate factors of camera resolution and selected area resolution
                    var restrictedZoneHeight = Mathf.Abs(restrictionTopLeft.y - restrictionBottomRight.y);
                    var restrictedZoneWidth = Mathf.Abs(restrictionTopLeft.x - restrictionBottomRight.x);
                    factorY =  Convert.ToSingle(_cameraData.CAMERA_HEIGHT) / restrictedZoneHeight;
                    factorX =  Convert.ToSingle(_cameraData.CAMERA_WIDTH) / restrictedZoneWidth;

                    Debug.LogWarning($"Scaling factor is {factorX}:{factorY}");
                    EndCalibration();
                }
            }
            else if (Input.GetMouseButtonDown(2))
            {
                RestartCalibration();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelCalibration();
            }
        }
    }

    private void Initialize()
    {
        isCalibrating = false;
        firstClick = true;
        _windowData = _laserPointerInputManager.GetWindowData();
        _cameraData = _laserPointerInputManager.GetCameraData();

    }

    private void StartCalibration()
    {
        VirtualMouse.instance.ShowCameraFeed();
        Debug.Log("Calibrating started.");
        Debug.Log("Please select 2 points TL and BR.");
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
            factorY = factorY
        };

        _laserPointerInputManager.SetCalibrationData(_calibrationData);
    }
}
