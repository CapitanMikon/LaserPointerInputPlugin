using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LPIPRunningState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
    private bool beginNoLaserProcedure = false;
    
    private const int EMPTY_FRAMES_THRESHOLD = 5;
    private int emptyFrames = 0;

    private int detectedFrames = 0; // detect laser and fire event every 9.8 frames avg z toho ako dlho svieti laser

    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private LPIPCalibrationData _lpipCalibrationData;
    private WindowData _windowData;

    private Vector3 _lastPosition;

    private ComputeShader _computeShader;
    private int kernelHandle;
    private RenderTexture outputTex;
    private Texture2D tex;

    private Bound[] _bounds;
    
    
    //
    private Vector2 idealAvg;
    private Vector2 idealStd;
    
    private Vector2 realAvg;
    private Vector2 realStd;

    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("Entered state {LPIPRunningState}");
        _lpipCoreManager = lpipCoreManager;

        webCamTexture = _lpipCoreManager.WebCamTexture;
        _lpipCalibrationData = _lpipCoreManager.LpipCalibrationData;

        outputTex = new RenderTexture(1920, 1080, 0);
        outputTex.enableRandomWrite = true;
        outputTex.Create();
        
        tex = new Texture2D(outputTex.width, outputTex.height);
        
        _computeShader = _lpipCoreManager.computeShader;
        kernelHandle = _computeShader.FindKernel("CSMain");
        
        _computeShader.SetTexture(kernelHandle ,"inputTexture", webCamTexture);
        _computeShader.SetTexture(kernelHandle ,"outputTexture", outputTex);
        _computeShader.SetFloats("dimensions", outputTex.width, outputTex.height);
        
        var rgbValues = lpipCoreManager.GetMaxAllowedRGBValues();
        _computeShader.SetFloats("maxRGBValues",rgbValues.x , rgbValues.y, rgbValues.z);

        var component = _lpipCoreManager.copy.GetComponent<RawImage>();
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

        _windowData = _lpipCoreManager.WindowData;
        
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
                _lpipCoreManager.InvokeOnLaserHitUpDetectedEvent(_lastPosition);
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }

        //bool updateMarker = !(_bounds[0].minX == outputTex.width + 1  && _bounds[0].minY == outputTex.height + 1);
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
        var component = _lpipCoreManager.copy.GetComponent<RawImage>();
        component.material.mainTexture = null;
        outputTex.Release();
        Debug.Log("Leaving state {LPIPRunningState}");
        StopLaserDetection();
    }
    
    public void StartLaserDetection()
    {
        _lpipCoreManager.InvokeDetectionStartedEvent();
        Debug.Log("Started laser detection.");
    }
    
    public void StopLaserDetection()
    {
        _lpipCoreManager.InvokeDetectionStoppedEvent();
        Debug.Log("Stopped laser detection."); 
    }

    private void UpdateMarkerImage()
    {
        Debug.Log("Marker was updated!");
        var centerX = (_bounds[0].minX + _bounds[0].maxX) / 2;
        var centerY = (_bounds[0].minY + _bounds[0].maxY) / 2;

        var transformedY = Mathf.Max(centerY - _lpipCalibrationData.restrictionBottomRight.y, 0) * _lpipCalibrationData.factorY * _windowData.GAME_WINDOW_FACTORY;
        var transformedX = Mathf.Max(centerX - _lpipCalibrationData.restrictionTopLeft.x, 0) * _lpipCalibrationData.factorX * _windowData.GAME_WINDOW_FACTORX;

        var result = new Vector2(transformedX, transformedY);
        /*var transformedY = (centerY) * _lpipCalibrationData.factorY * _windowData.GAME_WINDOW_FACTORY;
        var transformedX = (centerX) * _lpipCalibrationData.factorX * _windowData.GAME_WINDOW_FACTORX;
        
        
        var result = new Vector2(transformedX - _lpipCalibrationData.restrictionTopLeft.x, transformedY - _lpipCalibrationData.restrictionBottomRight.y);*/
        _lastPosition = result;
        _lpipCoreManager.InvokeOnLaserHitDownDetectedEvent(result);
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