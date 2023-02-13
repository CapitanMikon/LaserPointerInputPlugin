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

    private const double R_VALUE = 0.299;
    private const double G_VALUE = 0.587;
    private const double B_VALUE = 0.114;
    private const double PIXEL_LUMINANCE_TRESHOLD = 100;

    private Border top;
    private Border bottom;
    private Border left;
    private Border right;

    [SerializeField] private GameObject markerSprite;
    private RectTransform markerTransform;
    //[SerializeField] private TextMeshProUGUI text;
    
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
        top = new Border(-1, 1920, 0, 0);
        bottom = new Border(-1, 0, 0, 0);
        left = new Border(0, -1, 0, 0);
        right = new Border(1080, -1, 0, 0);
        
        markerTransform = markerSprite.GetComponent<RectTransform>();
        
        //laserDetectorCoroutine = GreyedImageProcess();
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

    /*IEnumerator GreyedImageProcess()
    {
        for (; ; )
        {
            yield return new WaitForEndOfFrame();
            webCamPixels = webCamTexture.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                webCamPixels[i].r = webCamPixels[i].g = webCamPixels[i].b = (webCamPixels[i].r + webCamPixels[i].g + webCamPixels[i].b) / 3;
            }
            resultImage.SetPixels(webCamPixels);
            resultImage.Apply();
            outputImageHolder.texture = resultImage;
        }
    }*/
    
    IEnumerator LaserPointerPositionUpdaterProcess()
    {
        for (; ; )
        {
            yield return new WaitForEndOfFrame();
            webCamPixels = webCamTexture.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                var pixelLuminance = CalculateLuminance(webCamPixels[i]);
                int currentX = i % webCamTexture.requestedWidth;
                int currentY = i / webCamTexture.requestedWidth;

                if (pixelLuminance > PIXEL_LUMINANCE_TRESHOLD)
                {
                    //UpdateBorders(currentX, currentY, pixelLuminance);
                    markerSprite.transform.position = new Vector3(currentX, currentY, 0);
                }

            }

            //UpdatePointerImage();
            //ResetBorders();
        }
    }

    private void UpdateBorders(int x, int y, double luminance)
    {
        string t = "";
        //top
        if (top.GetLuminance() < luminance && y < top.GetPosY())
        {
            top.SetPos(0,y);
            top.SetLuminance(luminance);
            t += "TOP: " + y + "; lum: \n" + luminance;
        }
        
        //bottom
        if (bottom.GetLuminance() < luminance && y > bottom.GetPosY())
        {
            bottom.SetPos(0, y);
            bottom.SetLuminance(luminance);
            t += "BOTTOM: " + y + "; lum: \n" + luminance;
        }
        
        //left
        if (left.GetLuminance() < luminance && x < left.GetPosX())
        {
            left.SetPos(x, 0);
            left.SetLuminance(luminance);
            t += "LEFT: " + x + "; lum: \n" + luminance;
        }
        
        //right
        if (right.GetLuminance() < luminance && x > right.GetPosX())
        {
            right.SetPos(x, 0);
            right.SetLuminance(luminance);
            t += "RIGHT: " + x + "; lum: \n" + luminance;
        }

        if (!t.Equals(""))
        {
            Debug.Log(t);
        }
        //text.text = t;
    }

    private static double CalculateLuminance(Color32 color32)
    {
        return R_VALUE * color32.r+ G_VALUE * color32.g + B_VALUE * color32.b;
    }

    private void UpdatePointerImage()
    {
        var centerX = right.GetPosX() - left.GetPosX();
        var centerY = top.GetPosY() - bottom.GetPosY();
        markerSprite.transform.localPosition = new Vector3(centerX, centerY, 0);
    }

    void ResetBorders()
    {
        top.SetValues(0, 1920, -1);
        bottom.SetValues(0, -1, -1);
        left.SetValues(-1, 0, -1);
        right.SetValues(1080, 0, -1);
    }
}

public class Border
{
    private double luminance;
    
    private int posX;
    private int posY;

    private int posIn2dArray;

    public Border(int x, int y, float luminance)
    {
        posX = x;
        posY = y;
        this.luminance = luminance;
    }
    
    public Border(int x, int y, float luminance, int posIn2dArray)
    {
        posX = x;
        posY = y;
        this.luminance = luminance;
        this.posIn2dArray = posIn2dArray;
    }

    public double GetLuminance()
    {
        return luminance;
    }

    public int GetPosX()
    {
        return posX;
    }
    public int GetPosY()
    {
        return posY;
    }

    public void SetLuminance(double luminance)
    {
        this.luminance = luminance;
    }

    
    public void SetPos(int x, int y)
    {
        posX = x;
        posY = y;
    }
    
    public void SetValues(int x, int y, double luminance)
    {
        posX = x;
        posY = y;
        this.luminance = luminance;
    }

    public void SetPosIn2dArray(int posIn2dArray)
    {
        this.posIn2dArray = posIn2dArray;
    }
}