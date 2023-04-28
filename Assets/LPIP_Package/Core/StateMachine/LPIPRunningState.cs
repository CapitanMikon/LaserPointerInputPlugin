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

        Init();
        
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
        
        var result = Project(centerX, centerY);
        _lastPosition = result;
        _lpipCoreManager.InvokeOnLaserHitDownDetectedEvent(result);
    }

    private void Init()
    {
        idealAvg = new Vector2(0, 0);
        for (int i = 0; i < _lpipCalibrationData.ideal.Length; i++)
        {
            idealAvg += _lpipCalibrationData.ideal[i];

            realAvg += _lpipCalibrationData.real[i];
        }

        idealAvg /= _lpipCalibrationData.ideal.Length;
        realAvg /= _lpipCalibrationData.real.Length;
        
        //calculate std

        var sodIdealX = 0.0f;
        var sodIdealY = 0.0f;
        
        var sodRealX = 0.0f;
        var sodRealY = 0.0f;
        for (int i = 0; i < _lpipCalibrationData.ideal.Length; i++)
        {
            sodIdealX += _lpipCalibrationData.ideal[i].x * _lpipCalibrationData.ideal[i].x;
            sodIdealY += _lpipCalibrationData.ideal[i].y * _lpipCalibrationData.ideal[i].y;
            
            sodRealX += _lpipCalibrationData.real[i].x * _lpipCalibrationData.real[i].x;
            sodRealY += _lpipCalibrationData.real[i].y * _lpipCalibrationData.real[i].y;
        }

        float sodAvgIdealX = sodIdealX / _lpipCalibrationData.ideal.Length;
        float sodAvgIdealY = sodIdealY / _lpipCalibrationData.ideal.Length;
        
        float sodAvgRealX = sodRealX / _lpipCalibrationData.real.Length;
        float sodAvgRealY = sodRealY / _lpipCalibrationData.real.Length;

        idealStd = new Vector2(Mathf.Sqrt(sodAvgIdealX - (idealAvg.x*idealAvg.x)), Mathf.Sqrt(sodAvgIdealY - (idealAvg.y*idealAvg.y)));
        realStd = new Vector2(Mathf.Sqrt(sodAvgRealX - (realAvg.x*realAvg.x)), Mathf.Sqrt(sodAvgRealY - (realAvg.y*realAvg.y)));

        
        for (int i = 0; i < _lpipCalibrationData.ideal.Length; i++)
        {
            var tmp = (_lpipCalibrationData.ideal[i] - idealAvg) / idealStd;
            _lpipCalibrationData.ideal[i] = tmp;
            
            tmp = (_lpipCalibrationData.real[i] - realAvg) / realStd;
            _lpipCalibrationData.real[i] = tmp;
        }
    }
    
    Vector2 Project(float x, float y)
    {
        var a1 = new Vector2(x, y);
        var a2 = a1 - realAvg;
        var a3 = a2 / realStd;

        var d = 1f;
        
        
        // q @ real.T
        var mat = new Vector4()
        {
            x = _lpipCalibrationData.real[0].x * a3.x + _lpipCalibrationData.real[0].y * a3.y,
            y = _lpipCalibrationData.real[1].x * a3.x + _lpipCalibrationData.real[1].y * a3.y,
            z = _lpipCalibrationData.real[2].x * a3.x + _lpipCalibrationData.real[2].y * a3.y,
            w = _lpipCalibrationData.real[3].x * a3.x + _lpipCalibrationData.real[3].y * a3.y,
        };

        var mat2 = mat / d;
        
        var c = SoftMax(mat2);
        
        var p = new Vector2()
        {
            x = c.x * _lpipCalibrationData.ideal[0].x + c.y * _lpipCalibrationData.ideal[1].x + c.z * _lpipCalibrationData.ideal[2].x + c.w * _lpipCalibrationData.ideal[3].x,
            y = c.x * _lpipCalibrationData.ideal[0].y + c.y * _lpipCalibrationData.ideal[1].y + c.z * _lpipCalibrationData.ideal[2].y + c.w * _lpipCalibrationData.ideal[3].y,
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

    private void ResetBound()
    {
        _bounds[0].minX = outputTex.width + 1;
        _bounds[0].minY = outputTex.height + 1;
        _bounds[0].maxX = 0;
        _bounds[0].maxY = 0;
        _bounds[0].detected = 0;
    }
}