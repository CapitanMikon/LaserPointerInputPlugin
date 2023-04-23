using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LPIPRunningState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
    private bool laserDetectionIsEnabled = false;
    private bool beginNoLaserProcedure = false;
    
    private const int EMPTY_FRAMES_THRESHOLD = 5;
    private int emptyFrames = 0;

    private int detectedFrames = 0; // detect laser and fire event every 9.8 frames avg z toho ako dlho svieti laser
    
    //source of const: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
    private const double R_VALUE = 0.299;
    private const double G_VALUE = 0.587;
    private const double B_VALUE = 0.114;
    private const double PIXEL_LUMINANCE_THRESHOLD = 160;
    
    private BorderPoint top;
    private BorderPoint bottom;
    private BorderPoint left;
    private BorderPoint right;

    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private LPIPCalibrationData _lpipCalibrationData;
    private CameraData _cameraData;
    private WindowData _windowData;

    private Vector3 _lastPosition;

    private ComputeShader _computeShader;
    private int kernelHandle;
    private RenderTexture outputTex;
    private Texture2D tex;

    private Bound[] _bounds;

    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("Entered state {LPIPRunningState}");
        _lpipCoreManager = lpipCoreManager;

        webCamTexture = _lpipCoreManager.WebCamTexture;
        _lpipCalibrationData = _lpipCoreManager.LpipCalibrationData;
        _cameraData = _lpipCoreManager.CameraData;
        _windowData = _lpipCoreManager.WindowData;

        outputTex = new RenderTexture(1920, 1080, 0);
        outputTex.enableRandomWrite = true;
        outputTex.Create();
        
        tex = new Texture2D(outputTex.width, outputTex.height);
        
        _computeShader = _lpipCoreManager.computeShader;
        kernelHandle = _computeShader.FindKernel("CSMain");
        _computeShader.SetTexture(kernelHandle ,"inputTexture", webCamTexture);
        _computeShader.SetTexture(kernelHandle ,"outputTexture", outputTex);
        _computeShader.SetFloats("dimensions", outputTex.width, outputTex.height);

        var component = _lpipCoreManager.copy.GetComponent<RawImage>();
        component.texture = outputTex;

        _bounds = new Bound[]{new Bound
            {
                minX = outputTex.width + 1,
                minY = outputTex.height + 1,
                maxX = 0,
                maxY = 0,
            }
        };

        ResetBorders();
        StartLaserDetection();
    }

    public override void UpdateState()
    {
        ComputeBuffer computeBuffer = new ComputeBuffer(_bounds.Length, sizeof(int)*4);
        computeBuffer.SetData(_bounds);
        _computeShader.SetBuffer(kernelHandle, "bounds", computeBuffer);
        _computeShader.Dispatch(kernelHandle, Mathf.CeilToInt(outputTex.width / 8f), Mathf.CeilToInt(outputTex.height / 8f),1); 
        
        computeBuffer.GetData(_bounds);

        computeBuffer.Dispose();
        if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                _lpipCoreManager.InvokeOnLaserHitUpDetectedEvent(_lastPosition);
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }
        
        bool updateMarker = !(_bounds[0].minX == outputTex.width + 1  && _bounds[0].minY == outputTex.height + 1);

        if (updateMarker)
        {
            UpdateMarkerImage();
            beginNoLaserProcedure = true;
            detectedFrames++;
            Debug.LogWarning($"Coords: {_bounds[0].minX}, {_bounds[0].maxX}, {_bounds[0].minY}, {_bounds[0].maxY}");
        }
        else if(detectedFrames != 0)
        {
            Debug.LogWarning($"No of Frames that detected laser: {detectedFrames}!");
            detectedFrames = 0;
        }

        ResetBound();
        /*if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                _lpipCoreManager.InvokeOnLaserHitUpDetectedEvent(_lastPosition);
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }
        
        if (laserDetectionIsEnabled)
        {
            bool updateMarker = false;
            GetRTPixels();
            webCamPixels = tex.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                //var pixelLuminance = R_VALUE * webCamPixels[i].r+ G_VALUE * webCamPixels[i].g + B_VALUE * webCamPixels[i].b;
                int currentX = i % _cameraData.CAMERA_WIDTH;
                int currentY = i / _cameraData.CAMERA_WIDTH;
                if (true)//(currentX >= _lpipCalibrationData.restrictionTopLeft.x && currentX <= _lpipCalibrationData.restrictionBottomRight.x
                   //                                                   && currentY >= _lpipCalibrationData.restrictionBottomRight.y && currentY <= _lpipCalibrationData.restrictionTopLeft.y)//is within restrictions
                {
                    if(webCamPixels[i].r > 120 && webCamPixels[i].g < 189  && webCamPixels[i].b < 170)
                    {
                        //Debug.LogWarning($"PIXEL: {currentX},{currentY}");
                            UpdateBorders(currentX, currentY);
                            updateMarker = true;
                    }
                }

            }

            if (updateMarker)
            {
                UpdateMarkerImage();
                updateMarker = false;
                beginNoLaserProcedure = true;
                detectedFrames++;
            }
            else if(detectedFrames != 0)
            {
                Debug.LogWarning($"No of Frames that detected laser: {detectedFrames}!");
                detectedFrames = 0;
            }
        
            ResetBorders();
        }*/

    }

    private void OnDestroy()
    {
        outputTex.Release();
    }

    public override void ExitState()
    {
        var component = _lpipCoreManager.copy.GetComponent<RawImage>();
        component.material.mainTexture = null;
        outputTex.Release();
        Debug.Log("Leaving state {LPIPRunningState}");
    }
    
    public void StartLaserDetection()
    {
        laserDetectionIsEnabled = true;
        _lpipCoreManager.InvokeDetectionStartedEvent();
        Debug.Log("Started laser detection.");
    }
    
    public void StopLaserDetection()
    {
        laserDetectionIsEnabled = false;
        _lpipCoreManager.InvokeDetectionStoppedEvent();
        Debug.Log("Stopped laser detection."); 
    }
    
    private void UpdateBorders(int x, int y)
    {
        //top
        if (y > top.value)
        {
            top.value = y;
        }
        
        //bottom
        if (y < bottom.value)
        {
            bottom.value = y;
        }
        
        //left
        if (x < left.value)
        {
            left.value = x;
        }
        
        //right
        if (x > right.value)
        {
            right.value = x;
        }
    }

    private void UpdateMarkerImage()
    {
        Debug.Log("Marker was updated!");
        var centerX = (_bounds[0].minX + _bounds[0].maxX) / 2;
        var centerY = (_bounds[0].minY + _bounds[0].maxY) / 2;
        
        var result = Project(new Vector2(centerX, centerY));
        _lastPosition = result;
        _lpipCoreManager.InvokeOnLaserHitDownDetectedEvent(result);
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
            //DebugTextController.Instance.ResetText(DebugTextController.DebugTextGroup.Side);
            //DebugTextController.Instance.AppendText("Top",DebugTextController.DebugTextGroup.Side);
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
            //DebugTextController.Instance.ResetText(DebugTextController.DebugTextGroup.Side);
            //DebugTextController.Instance.AppendText("Bot",DebugTextController.DebugTextGroup.Side);
            var k = Triangle(pts,pos);
            return Dot(pts2,k);
        }
    }
    
    private void GetRTPixels()
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = outputTex;

        // Create a new Texture2D and read the RenderTexture image into it
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        // Restore previously active render texture
        RenderTexture.active = currentActiveRT;
    }

    private void ResetBound()
    {
        _bounds[0].minX = outputTex.width + 1;
        _bounds[0].minY = outputTex.height + 1;
        _bounds[0].maxX = 0;
        _bounds[0].maxY = 0;
    }
}

public struct Bound {
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;
};