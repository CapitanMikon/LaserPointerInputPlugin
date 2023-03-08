using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LaserPointerInputInOperation : LaserPointerInputBaseState
{
    private LaserPointerInputManager _laserPointerInputManager;
    
    private bool laserDetectionIsEnabled = false;
    private bool beginNoLaserProcedure = false;
    
    private const int EMPTY_FRAMES_THRESHOLD = 5;
    private int emptyFrames = 0;
    
    //source of const: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
    private const double R_VALUE = 0.299;
    private const double G_VALUE = 0.587;
    private const double B_VALUE = 0.114;
    private const double PIXEL_LUMINANCE_THRESHOLD = 100;
    
    private BorderPoint top;
    private BorderPoint bottom;
    private BorderPoint left;
    private BorderPoint right;

    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private CallibrationData _callibrationData;
    private CameraData _cameraData;
    private WindowData _windowData;
    
    public override void EnterState(LaserPointerInputManager laserPointerInputManager)
    {
        Debug.Log("STATE: OPERATION");
        _laserPointerInputManager = laserPointerInputManager;

        webCamTexture = _laserPointerInputManager.webCamTexture;
        _callibrationData = _laserPointerInputManager.GetCallibrationData();
        _cameraData = _laserPointerInputManager.GetCameraData();
        _windowData = _laserPointerInputManager.GetWindowData();
        
        ResetBorders();
        StartLaserDetection();
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            VirtualMouse.instance.ShowCameraFeed();
        }else if(Input.GetKeyDown(KeyCode.H))
        {
            VirtualMouse.instance.HideCameraFeed();
        }else if (Input.GetKeyDown(KeyCode.R))
        {
            _laserPointerInputManager.SwitchState(_laserPointerInputManager.calibrationState);
        }
        if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                _laserPointerInputManager.ResetMarkerSpritePosition();
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }
        
        if (laserDetectionIsEnabled)
        {
            bool updateMarker = false;
            webCamPixels = webCamTexture.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                var pixelLuminance = R_VALUE * webCamPixels[i].r+ G_VALUE * webCamPixels[i].g + B_VALUE * webCamPixels[i].b;
                int currentX = i % _cameraData.CAMERA_WIDTH;
                int currentY = i / _cameraData.CAMERA_WIDTH;
                if (currentX >= _callibrationData.restrictionTopLeft.x && currentX <= _callibrationData.restrictionBottomRight.x
                                                                      && currentY >= _callibrationData.restrictionBottomRight.y && currentY <= _callibrationData.restrictionTopLeft.y)//is within restrictions
                {
                    if (pixelLuminance > PIXEL_LUMINANCE_THRESHOLD)
                    {
                        //Debug.LogWarning($"PIXEL: {currentX},{currentY}");
                        UpdateBorders(currentX, currentY, pixelLuminance);
                        updateMarker = true;
                    }
                }

            }

            if (updateMarker)
            {
                UpdateMarkerImage();
                updateMarker = false;
                beginNoLaserProcedure = true;
            }
        
            ResetBorders();
        }
        
    }
    
    public void StartLaserDetection()
    {
        laserDetectionIsEnabled = true;
        Debug.Log("Started laser detection.");
    }
    
    public void StopLaserDetection()
    {
        laserDetectionIsEnabled = false;
        Debug.Log("Stopped laser detection.");
        _laserPointerInputManager.ResetMarkerSpritePosition();
    }
    
    private void UpdateBorders(int x, int y, double luminance)
    {
        //top
        if (top.luminance< luminance && y > top.value)
        {
            top.value = y;
            top.luminance = luminance;
        }
        
        //bottom
        if (bottom.luminance < luminance && y < bottom.value)
        {
            bottom.value = y;
            bottom.luminance = luminance;
        }
        
        //left
        if (left.luminance < luminance && x < left.value)
        {
            left.value = x;
            left.luminance = luminance;
        }
        
        //right
        if (right.luminance < luminance && x > right.value)
        {
            right.value = x;
            right.luminance = luminance;
        }
    }

    private void UpdateMarkerImage()
    {
        Debug.Log("Marker was updated!");
        var centerX = (left.value + right.value) / 2;
        var centerY = (top.value + bottom.value) / 2;

        var transformedY = Mathf.Max(centerY - _callibrationData.restrictionBottomRight.y, 0) * _callibrationData.factorY * _windowData.GAME_WINDOW_FACTORY;
        var transformedX = Mathf.Max(centerX - _callibrationData.restrictionTopLeft.x, 0) * _callibrationData.factorX * _windowData.GAME_WINDOW_FACTORX;
        
        VirtualMouse.instance.SetMouseClickPositions(transformedX, transformedY);
        _laserPointerInputManager.InvokeOnLaserPointerInputDetectedEvent();

        _laserPointerInputManager.UpdateMarkerSpritePosition(transformedX, transformedY);
    }

    void ResetBorders()
    {
        (top.value, top.luminance) =(0, -1);
        (bottom.value, bottom.luminance) = (_cameraData.CAMERA_HEIGHT, -1);
        
        (left.value, left.luminance) = (_cameraData.CAMERA_WIDTH, -1);
        (right.value, right.luminance) = ( 0, -1);
    }
}
