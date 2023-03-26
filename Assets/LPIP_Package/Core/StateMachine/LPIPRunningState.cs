using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LPIPRunningState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
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
    
    private LPIPCalibrationData _lpipCalibrationData;
    private CameraData _cameraData;
    private WindowData _windowData;
    
    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("LPIP currentstate = {LPIPRunningState}");
        _lpipCoreManager = lpipCoreManager;

        webCamTexture = _lpipCoreManager.webCamTexture;
        _lpipCalibrationData = _lpipCoreManager.GetCalibrationData();
        _cameraData = _lpipCoreManager.GetCameraData();
        _windowData = _lpipCoreManager.GetWindowData();
        
        ResetBorders();
        StartLaserDetection();
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LPIPMouseEmulation.Instance.ShowCameraFeed();
        }else if(Input.GetKeyDown(KeyCode.F2))
        {
            LPIPMouseEmulation.Instance.HideCameraFeed();
        }else if (Input.GetKeyDown(KeyCode.F5))
        {
            _lpipCoreManager.SwitchState(_lpipCoreManager.ManualCalibrationState);
        }
        if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                _lpipCoreManager.ResetLaserMarkerPos();
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
                if (true)//(currentX >= _lpipCalibrationData.restrictionTopLeft.x && currentX <= _lpipCalibrationData.restrictionBottomRight.x
                   //                                                   && currentY >= _lpipCalibrationData.restrictionBottomRight.y && currentY <= _lpipCalibrationData.restrictionTopLeft.y)//is within restrictions
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
        _lpipCoreManager.ResetLaserMarkerPos();
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

        var transformedY = Mathf.Max(centerY - _lpipCalibrationData.restrictionBottomRight.y, 0) * _lpipCalibrationData.factorY * _windowData.GAME_WINDOW_FACTORY;
        var transformedX = Mathf.Max(centerX - _lpipCalibrationData.restrictionTopLeft.x, 0) * _lpipCalibrationData.factorX * _windowData.GAME_WINDOW_FACTORX;
        
        var result = Project(new Vector2(centerX, centerY));
        
        LPIPMouseEmulation.Instance.SetMouseClickPositions(result.x, result.y);
        _lpipCoreManager.InvokeOnLaserPointerInputDetectedEvent();

        _lpipCoreManager.UpdateLaserMarkerPos(result.x, result.y);
    }

    void ResetBorders()
    {
        (top.value, top.luminance) =(0, -1);
        (bottom.value, bottom.luminance) = (_cameraData.CAMERA_HEIGHT, -1);
        
        (left.value, left.luminance) = (_cameraData.CAMERA_WIDTH, -1);
        (right.value, right.luminance) = ( 0, -1);
    }
    float Determinant(float a, float b, float c, float d)
    {
        return a * d - b * c;
    }

    Vector2 Solve(Vector2[] A, Vector2 B)
    {
        var det = Determinant(A[0][0], A[1][0], A[0][1], A[1][1]);
        var x = Determinant(B[0], A[1][0], B[1], A[1][1]) / det;
        var y = Determinant(A[0][0], B[0], A[0][1], B[1]) / det;
        return new Vector2(x,y);
    }

    Vector3 Triangle(Vector2[] pts, Vector2 p)
    {
        Vector2 a = new Vector2(pts[1][0] - pts[0][0], pts[1][1] - pts[0][1]);
        Vector2 b = new Vector2(pts[2][0] - pts[0][0], pts[2][1] - pts[0][1]);
        Vector2 c = new Vector2(p[0] - pts[0][0], p[1] - pts[0][1]);

        Vector2[] ab = new Vector2[2]
        {
            a,
            b
        };
        
        Vector2 d = Solve(ab,c);
        return new Vector3(1 - d[0] - d[1], d[0], d[1]);
    }

    Vector2 Dot(Vector2[] pts, Vector3 k)
    {
        return new Vector2(pts[0][0] * k[0] + pts[1][0] * k[1] + pts[2][0] * k[2],
            pts[0][1] * k[0] + pts[1][1] * k[1] + pts[2][1] * k[2]);
    }

    Vector2 Round(Vector2 t)
    {
        return new Vector2(Convert.ToInt32(t.x), Convert.ToInt32(t.y));
    }

    Vector2 Project(Vector2 pos)
    {
        var i = 0;
        var j = 2;
        //Vector2 n = new Vector2(real[i].y - real[j].y, real[j].x - real[i].x);
        Vector2 v = new Vector2(_lpipCalibrationData.real[i].x - _lpipCalibrationData.real[j].x, _lpipCalibrationData.real[i].y - _lpipCalibrationData.real[j].y);
        //var q = n.x * real[i].x + n.y * real[i].y;
        var q = (v.y * _lpipCalibrationData.real[j].x - v.x * _lpipCalibrationData.real[j].y) * -1;

        if (v.y * pos.x -v.x * pos.y + q >= 0)
        {
            Vector2[] pts = new Vector2[3]{
                _lpipCalibrationData.real[0],
                _lpipCalibrationData.real[2],
                _lpipCalibrationData.real[3]
            };
            Vector2[] pts2 = new Vector2[3]{
                _lpipCalibrationData.ideal[0],
                _lpipCalibrationData.ideal[2],
                _lpipCalibrationData.ideal[3]
            };
            var k = Triangle(pts,pos);
            //DebugText.Instance.ResetText(DebugText.DebugTextGroup.Side);
            //DebugText.Instance.AddText("Top",DebugText.DebugTextGroup.Side);
            return Dot(pts2,k);
        }
        else
        {
            Vector2[] pts = new Vector2[3]{
                _lpipCalibrationData.real[0],
                _lpipCalibrationData.real[2],
                _lpipCalibrationData.real[1]
            };
            Vector2[] pts2 = new Vector2[3]{
                _lpipCalibrationData.ideal[0],
                _lpipCalibrationData.ideal[2],
                _lpipCalibrationData.ideal[1]
            };
            //DebugText.Instance.ResetText(DebugText.DebugTextGroup.Side);
            //DebugText.Instance.AddText("Bot",DebugText.DebugTextGroup.Side);
            var k = Triangle(pts,pos);
            return Dot(pts2,k);
        }
    }
}
