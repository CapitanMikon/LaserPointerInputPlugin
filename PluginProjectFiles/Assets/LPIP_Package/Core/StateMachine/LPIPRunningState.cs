using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LPIPRunningState : LPIPBaseState
{
    private bool beginNoLaserProcedure = false;
    
    private const int EMPTY_FRAMES_THRESHOLD = 5;
    private int emptyFrames = 0;

    private int detectedFrames = 0; // detect laser and fire event every 9.8 frames

    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private LPIPCalibrationData _lpipCalibrationData;

    private Vector3 _lastPosition;

    private ComputeShader _computeShader;
    private int kernelHandle;
    private RenderTexture outputTex;
    private Texture2D tex;

    private Bound[] _bounds;

    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        //Debug.Log("Entered state {LPIPRunningState}");
        LpipCoreManager = lpipCoreManager;

        webCamTexture = LpipCoreManager.WebCamTexture;
        _lpipCalibrationData = LpipCoreManager.LpipCalibrationData;

        outputTex = new RenderTexture(1920, 1080, 0);
        outputTex.enableRandomWrite = true;
        outputTex.Create();
        
        tex = new Texture2D(outputTex.width, outputTex.height);
        
        _computeShader = LpipCoreManager.GetComputeShader();
        kernelHandle = _computeShader.FindKernel("CSMain");
        
        _computeShader.SetTexture(kernelHandle ,"inputTexture", webCamTexture);
        _computeShader.SetTexture(kernelHandle ,"outputTexture", outputTex);
        _computeShader.SetFloats("dimensions", outputTex.width, outputTex.height);
        _computeShader.SetFloats("areaOfInterestTop", _lpipCalibrationData.real[3].x, _lpipCalibrationData.real[3].y, _lpipCalibrationData.real[2].x, _lpipCalibrationData.real[2].y);
        _computeShader.SetFloats("areaOfInterestBottom", _lpipCalibrationData.real[0].x, _lpipCalibrationData.real[0].y, _lpipCalibrationData.real[1].x, _lpipCalibrationData.real[1].y);
        
        var rgbValues = lpipCoreManager.GetMaxAllowedRGBValues();
        _computeShader.SetFloats("maxRGBValues",rgbValues.x , rgbValues.y, rgbValues.z);

        var component = LpipCoreManager.GetRawImage().GetComponent<RawImage>();
        component.texture = outputTex;

        _bounds = new Bound[]{new Bound
            {
                minX = outputTex.width + 1,
                minY = outputTex.height + 1,
                maxX = 0,
                maxY = 0,
                detected = 0,
            }
        };

        ResetBound();
        StartLaserDetection();
    }

    public override void UpdateState()
    {
        ComputeBuffer computeBuffer = new ComputeBuffer(_bounds.Length, sizeof(int)*5);
        computeBuffer.SetData(_bounds);
        _computeShader.SetBuffer(kernelHandle, "bounds", computeBuffer);
        _computeShader.Dispatch(kernelHandle, Mathf.CeilToInt(outputTex.width / 8f), Mathf.CeilToInt(outputTex.height / 8f),1); 
        
        computeBuffer.GetData(_bounds);
        computeBuffer.Dispose();
        
        if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                LpipCoreManager.InvokeOnLaserHitUpDetectedEvent(_lastPosition);
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }
        
        bool updateMarker = _bounds[0].detected == 1 && (_bounds[0].minX >= 0 && _bounds[0].minY >= 0 && _bounds[0].maxX <= webCamTexture.width && _bounds[0].maxY <= webCamTexture.height);

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

    }

    private void OnDestroy()
    {
        outputTex.Release();
    }

    public override void ExitState()
    {
        var component = LpipCoreManager.GetRawImage().GetComponent<RawImage>();
        component.material.mainTexture = null;
        outputTex.Release();
        //Debug.Log("Leaving state {LPIPRunningState}");
        StopLaserDetection();
    }
    
    public void StartLaserDetection()
    {
        LpipCoreManager.InvokeDetectionStartedEvent();
        //Debug.Log("Started laser detection.");
    }
    
    public void StopLaserDetection()
    {
        LpipCoreManager.InvokeDetectionStoppedEvent();
        //Debug.Log("Stopped laser detection."); 
    }

    private void UpdateMarkerImage()
    {
        //Debug.Log("Marker was updated!");
        var centerX = (_bounds[0].minX + _bounds[0].maxX) / 2;
        var centerY = (_bounds[0].minY + _bounds[0].maxY) / 2;
        
        var result = Project(new Vector2(centerX, centerY));
        _lastPosition = result;
        LpipCoreManager.InvokeOnLaserHitDownDetectedEvent(result);
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
        Vector2 v = new Vector2(_lpipCalibrationData.real[i].x - _lpipCalibrationData.real[j].x, _lpipCalibrationData.real[i].y - _lpipCalibrationData.real[j].y);
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
            var k = Triangle(pts,pos);
            return Dot(pts2,k);
        }
    }

    private void ResetBound()
    {
        _bounds[0].minX = outputTex.width + 1;
        _bounds[0].minY = outputTex.height + 1;
        _bounds[0].maxX = 0;
        _bounds[0].maxY = 0;
        _bounds[0].detected = 0;
    }
}