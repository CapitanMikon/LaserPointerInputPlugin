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

    private Vector2 idealAvg;
    private Vector2 idealStd;
    
    private Vector2 realAvg;
    private Vector2 realStd;

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
        SetUp();
    }

    private void SetUp()
    {
        //
        
        /*_callibrationData.ideal = new Vector2[4]{
            new Vector2(0, 0),
            new Vector2(200, 0),
            new Vector2(200, 200),
            new Vector2(0, 200)
        };
        
        _callibrationData.real = new Vector2[4]{
            new Vector2(300, 100),
            new Vector2(600, 200),
            new Vector2(500, 400),
            new Vector2(350, 300)
        };*/
        
        //
        idealAvg = new Vector2(0, 0);
        for (int i = 0; i < _callibrationData.ideal.Length; i++)
        {
            idealAvg += _callibrationData.ideal[i];

            realAvg += _callibrationData.real[i];
        }

        idealAvg /= _callibrationData.ideal.Length;
        realAvg /= _callibrationData.real.Length;
        
        //calculate std

        var sodIdealX = 0.0f;
        var sodIdealY = 0.0f;
        
        var sodRealX = 0.0f;
        var sodRealY = 0.0f;
        for (int i = 0; i < _callibrationData.ideal.Length; i++)
        {
            sodIdealX += _callibrationData.ideal[i].x * _callibrationData.ideal[i].x;
            sodIdealY += _callibrationData.ideal[i].y * _callibrationData.ideal[i].y;
            
            sodRealX += _callibrationData.real[i].x * _callibrationData.real[i].x;
            sodRealY += _callibrationData.real[i].y * _callibrationData.real[i].y;
        }

        float sodAvgIdealX = sodIdealX / _callibrationData.ideal.Length;
        float sodAvgIdealY = sodIdealY / _callibrationData.ideal.Length;
        
        float sodAvgRealX = sodRealX / _callibrationData.real.Length;
        float sodAvgRealY = sodRealY / _callibrationData.real.Length;

        idealStd = new Vector2(Mathf.Sqrt(sodAvgIdealX - (idealAvg.x*idealAvg.x)), Mathf.Sqrt(sodAvgIdealY - (idealAvg.y*idealAvg.y)));
        realStd = new Vector2(Mathf.Sqrt(sodAvgRealX - (realAvg.x*realAvg.x)), Mathf.Sqrt(sodAvgRealY - (realAvg.y*realAvg.y)));

        
        for (int i = 0; i < _callibrationData.ideal.Length; i++)
        {
            var tmp = (_callibrationData.ideal[i] - idealAvg) / idealStd;
            _callibrationData.ideal[i] = tmp;
            
            tmp = (_callibrationData.real[i] - realAvg) / realStd;
            _callibrationData.real[i] = tmp;
        }
        
        //var test = Project(340, 200);
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            VirtualMouse.instance.ShowCameraFeed();
        }else if(Input.GetKeyDown(KeyCode.F2))
        {
            VirtualMouse.instance.HideCameraFeed();
        }else if (Input.GetKeyDown(KeyCode.F5))
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
                if (true)//(currentX >= _callibrationData.restrictionTopLeft.x && currentX <= _callibrationData.restrictionBottomRight.x
                   //                                                   && currentY >= _callibrationData.restrictionBottomRight.y && currentY <= _callibrationData.restrictionTopLeft.y)//is within restrictions
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
        
        var result = Project(centerX, centerY);
        
        VirtualMouse.instance.SetMouseClickPositions(result.x, result.y);
        _laserPointerInputManager.InvokeOnLaserPointerInputDetectedEvent();

        _laserPointerInputManager.UpdateMarkerSpritePosition(result.x, result.y);
    }

    void ResetBorders()
    {
        (top.value, top.luminance) =(0, -1);
        (bottom.value, bottom.luminance) = (_cameraData.CAMERA_HEIGHT, -1);
        
        (left.value, left.luminance) = (_cameraData.CAMERA_WIDTH, -1);
        (right.value, right.luminance) = ( 0, -1);
    }

    Vector2 Project(float x, float y)
    {
        var a1 = new Vector2(x, y);
        var a2 = a1 - realAvg;
        var a3 = a2 / realStd;

        var d = 0.5f;
        
        
        // q @ real.T
        var mat = new Vector4()
        {
            x = _callibrationData.real[0].x * a3.x + _callibrationData.real[0].y * a3.y,
            y = _callibrationData.real[1].x * a3.x + _callibrationData.real[1].y * a3.y,
            z = _callibrationData.real[2].x * a3.x + _callibrationData.real[2].y * a3.y,
            w = _callibrationData.real[3].x * a3.x + _callibrationData.real[3].y * a3.y,
        };

        var mat2 = mat / d;
        
        var c = SoftMax(mat2);
        
        var p = new Vector2()
        {
            x = c.x * _callibrationData.ideal[0].x + c.y * _callibrationData.ideal[1].x + c.z * _callibrationData.ideal[2].x + c.w * _callibrationData.ideal[3].x,
            y = c.x * _callibrationData.ideal[0].y + c.y * _callibrationData.ideal[1].y + c.z * _callibrationData.ideal[2].y + c.w * _callibrationData.ideal[3].y,
        };
        
        return (p * idealStd) + idealAvg;
    }

    private Vector4 SoftMax(Vector4 v)
    {
        var temp = new Vector4(0.0f,0.0f,0.0f,0.0f);
        for (int i = 0; i < 4; i++)
        {
            temp[i] = Mathf.Exp(v[i]);
        }

        var sum = temp.x + temp.y + temp.z + temp.w;
        
        return temp / sum;
    }
}
