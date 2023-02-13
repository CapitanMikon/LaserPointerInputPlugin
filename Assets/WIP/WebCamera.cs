using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera : MonoBehaviour
{
    [SerializeField] private RawImage webcamImageHolder;
    [SerializeField] private RawImage outputImageHolder;
    
    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private Texture2D resultImage;

    private IEnumerator laserDetectorCoroutine;

    //source of const: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
    private const double R_VALUE = 0.299;
    private const double G_VALUE = 0.587;
    private const double B_VALUE = 0.114;
    private const double PIXEL_LUMINANCE_TRESHOLD = 150;

    private BorderPoint top;
    private BorderPoint bottom;
    private BorderPoint left;
    private BorderPoint right;

    [SerializeField] private GameObject markerSprite;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log("camera <" + devices[i].name + "> detected");
        }

        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name);
            webcamImageHolder.texture = webCamTexture;
            ConfigureWebcam(1920, 1080, 60);
            webCamTexture.Play();
        }
        resultImage = new Texture2D(webCamTexture.width, webCamTexture.height);
        LogWebcamInfo();
        
        //setUp
        top = new BorderPoint( 0, 0, 0);
        bottom = new BorderPoint(1080, 0, 0);
        
        left = new BorderPoint(1920, 0, 0);
        right = new BorderPoint(0, 0, 0);
        
        laserDetectorCoroutine = LaserPointerPositionUpdaterProcess();
        StartCoroutine(laserDetectorCoroutine);
    }

    void ConfigureWebcam(int width, int height, int fps)
    {
        webCamTexture.requestedFPS = fps;
        webCamTexture.requestedWidth = width;
        webCamTexture.requestedHeight = height;
    }

    void LogWebcamInfo()
    {
        Debug.Log($"Current camera configuration:\nFPS: {webCamTexture.requestedFPS}\nRes: {webCamTexture.width}x{webCamTexture.height}");
    }

    private void Update()
    {
        
    }

    IEnumerator LaserPointerPositionUpdaterProcess()
    {
        int width = webCamTexture.requestedWidth;
        bool updateMarker = false;
        for (; ; )
        {
            yield return new WaitForEndOfFrame();
            webCamPixels = webCamTexture.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                var pixelLuminance = R_VALUE * webCamPixels[i].r+ G_VALUE * webCamPixels[i].g + B_VALUE * webCamPixels[i].b;
                int currentX = i % width;
                int currentY = i / width;

                if (pixelLuminance > PIXEL_LUMINANCE_TRESHOLD)
                {
                    //Debug.LogWarning($"PIXEL: {currentX},{currentY}");
                    UpdateBorders(currentX, currentY, pixelLuminance);
                    updateMarker = true;
                }

            }

            if (updateMarker)
            {
                UpdateMarkerImage();
                updateMarker = false;
            }
            ResetBorders();
        }
    }

    private void UpdateBorders(int x, int y, double luminance)
    {
        //top
        if (top.GetLuminance() < luminance && y > top.GetPos())
        {
            top.SetPos(y);
            top.SetLuminance(luminance);
        }
        
        //bottom
        if (bottom.GetLuminance() < luminance && y < bottom.GetPos())
        {
            bottom.SetPos(y);
            bottom.SetLuminance(luminance);
        }
        
        //left
        if (left.GetLuminance() < luminance && x < left.GetPos())
        {
            left.SetPos(x);
            left.SetLuminance(luminance);
        }
        
        //right
        if (right.GetLuminance() < luminance && x > right.GetPos())
        {
            right.SetPos(x);
            right.SetLuminance(luminance);
        }
    }

    /*private static double CalculateLuminance(Color32 color32)
    {
        return R_VALUE * color32.r+ G_VALUE * color32.g + B_VALUE * color32.b;
    }*/

    private void UpdateMarkerImage()
    {
        var centerX = (left.GetPos() + right.GetPos()) / 2;
        var centerY = (top.GetPos() + bottom.GetPos()) / 2;
        markerSprite.transform.position = new Vector3(centerX, centerY, 0);
    }

    void ResetBorders()
    {
        top.SetValues(0, -1);
        bottom.SetValues(1080, -1);
        
        left.SetValues(1920, -1);
        right.SetValues( 0, -1);
    }
}

public class BorderPoint
{
    private double luminance;
    private int value;

    private int posIn2dArray;

    public BorderPoint(int x, float luminance)
    {
        value = x;
        this.luminance = luminance;
    }
    
    public BorderPoint(int x, float luminance, int posIn2dArray)
    {
        value = x;
        this.luminance = luminance;
        this.posIn2dArray = posIn2dArray;
    }

    public double GetLuminance()
    {
        return luminance;
    }

    public int GetPos()
    {
        return value;
    }

    public void SetLuminance(double luminance)
    {
        this.luminance = luminance;
    }

    
    public void SetPos(int x)
    {
        value = x;
    }
    
    public void SetValues(int x, double luminance)
    {
        value = x;
        this.luminance = luminance;
    }

    public void SetPosIn2dArray(int posIn2dArray)
    {
        this.posIn2dArray = posIn2dArray;
    }
}