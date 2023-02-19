using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera : MonoBehaviour
{
    [SerializeField] private RawImage webcamImageHolder;
    
    private WebCamTexture webCamTexture;
    private Color32[] webCamPixels;
    
    private IEnumerator laserDetectorCoroutine;

    //source of const: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
    private const double R_VALUE = 0.299;
    private const double G_VALUE = 0.587;
    private const double B_VALUE = 0.114;
    private const double PIXEL_LUMINANCE_THRESHOLD = 100;
    private const int EMPTY_FRAMES_THRESHOLD = 5;

    private int CAMERA_WIDTH = -1;
    private int CAMERA_HEIGHT = -1;
    private int CAMERA_FPS = -1;

    
    //scaling from game to camera
    private int GAME_WINDOW_WIDTH = -1;
    private int GAME_WINDOW_HEIGHT = -1;
    private float GAME_WINDOW_FACTORX = -1;
    private float GAME_WINDOW_FACTORY = -1;

    //border points for detected laser dot
    private BorderPoint top;
    private BorderPoint bottom;
    private BorderPoint left;
    private BorderPoint right;
    
    private int emptyFrames = 0;
    private Vector3 markerDefaultPosition = new Vector3(-100, -100, 0);
    private bool beginNoLaserProcedure;

    [SerializeField] private GameObject markerSprite;
    
    
    //scaling from camera (smaller projected image) to computer screen
    private Pair restrictionTopLeft = new Pair();
    private Pair restrictionBottomRight = new Pair();

    private bool firstClick = true;
    private bool isCalibrating = true;

    private float factorX;
    private float factorY;

    void Start()
    {
        ResetMarkerImagePos();
        
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
            //ConfigureWebcam(1280, 720, 30);
            webCamTexture.Play();
        }
        LogWebcamInfo();
        
        //get real camera/game width and height
        CAMERA_WIDTH = webCamTexture.requestedWidth;
        CAMERA_HEIGHT = webCamTexture.requestedHeight;

        GAME_WINDOW_HEIGHT = Display.main.systemHeight;
        GAME_WINDOW_WIDTH = Display.main.systemWidth;
        
        Debug.LogWarning($"Window res: {GAME_WINDOW_WIDTH}x{GAME_WINDOW_HEIGHT}");

        GAME_WINDOW_FACTORX =  GAME_WINDOW_WIDTH / Convert.ToSingle(CAMERA_WIDTH);
        GAME_WINDOW_FACTORY =  GAME_WINDOW_HEIGHT / Convert.ToSingle(CAMERA_HEIGHT);
        Debug.LogWarning($"Window res factor is: {GAME_WINDOW_FACTORX}:{GAME_WINDOW_FACTORY}");
        

        //set up borders
        top = new BorderPoint( 0, 0, 0);
        bottom = new BorderPoint(CAMERA_HEIGHT, 0, 0);
        
        left = new BorderPoint(CAMERA_WIDTH, 0, 0);
        right = new BorderPoint(0, 0, 0);
        
        laserDetectorCoroutine = LaserPointerPositionUpdaterProcess();
        //StartLaserDetection(); //we dont start it here now
    }

    void ConfigureWebcam(int width, int height, int fps)
    {
        webCamTexture.requestedFPS = fps;
        webCamTexture.requestedWidth = width;
        webCamTexture.requestedHeight = height;
    }

    public void StartLaserDetection()
    {
        Debug.Log("Started laser detection.");
        StartCoroutine(laserDetectorCoroutine);
    }
    
    public void StopLaserDetection()
    {
        Debug.Log("Stopped laser detection.");
        StopCoroutine(laserDetectorCoroutine);
        ResetMarkerImagePos();
    }

    void LogWebcamInfo()
    {
        Debug.Log($"Current camera configuration:\nFPS: {webCamTexture.requestedFPS}\nRes: {webCamTexture.width}x{webCamTexture.height}");
    }

    private void Update()
    {
        if (beginNoLaserProcedure)
        {
            if (emptyFrames >= EMPTY_FRAMES_THRESHOLD)
            {
                ResetMarkerImagePos();
                emptyFrames = 0;
                beginNoLaserProcedure = false;
            }

            emptyFrames++;
        }

        if (isCalibrating)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Debug.Log($"Mouse click at [{screenPosition.x}, {screenPosition.y}]");
                if (firstClick)
                {
                    restrictionTopLeft.x = Convert.ToInt32(screenPosition.x / GAME_WINDOW_FACTORX);
                    restrictionTopLeft.y = Convert.ToInt32(screenPosition.y / GAME_WINDOW_FACTORY);
                    firstClick = false;
                }
                else
                {
                    restrictionBottomRight.x = Convert.ToInt32(screenPosition.x / GAME_WINDOW_FACTORX);
                    restrictionBottomRight.y = Convert.ToInt32(screenPosition.y / GAME_WINDOW_FACTORY);
                    isCalibrating = false;
                    
                    //calculate factors of camera resolution and selected area resolution
                    var restrictedZoneHeight = Mathf.Abs(restrictionTopLeft.y - restrictionBottomRight.y);
                    var restrictedZoneWidth = Mathf.Abs(restrictionTopLeft.x - restrictionBottomRight.x);
                    factorY =  Convert.ToSingle(CAMERA_HEIGHT) / restrictedZoneHeight;
                    factorX =  Convert.ToSingle(CAMERA_WIDTH) / restrictedZoneWidth;

                    Debug.LogWarning($"Factor is {factorX}:{factorY}");
                    Debug.Log("Calibrating ended.");
                    StartLaserDetection();
                }
            }
        }
    }

    IEnumerator LaserPointerPositionUpdaterProcess()
    {
        bool updateMarker = false;
        for (; ; )
        {
            yield return new WaitForEndOfFrame();
            webCamPixels = webCamTexture.GetPixels32();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                var pixelLuminance = R_VALUE * webCamPixels[i].r+ G_VALUE * webCamPixels[i].g + B_VALUE * webCamPixels[i].b;
                int currentX = i % CAMERA_WIDTH;
                int currentY = i / CAMERA_WIDTH;
                if (currentX >= restrictionTopLeft.x && currentX <= restrictionBottomRight.x
                    && currentY >= restrictionBottomRight.y && currentY <= restrictionTopLeft.y)//is within restrictions
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
        Debug.Log("Marker was updated!");
        var centerX = (left.GetPos() + right.GetPos()) / 2;
        var centerY = (top.GetPos() + bottom.GetPos()) / 2;

        var transformedY = Mathf.Max(centerY - restrictionBottomRight.y, 0) * factorY * GAME_WINDOW_FACTORY;
        var transformedX = Mathf.Max(centerX - restrictionTopLeft.x, 0) * factorX * GAME_WINDOW_FACTORX;
        
        markerSprite.transform.position = new Vector3(transformedX, transformedY, 0);
    }

    private void ResetMarkerImagePos()
    {
        Debug.LogWarning("Marker was reset off screen!");
        markerSprite.transform.position = markerDefaultPosition;
    }

    void ResetBorders()
    {
        top.SetValues(0, -1);
        bottom.SetValues(CAMERA_HEIGHT, -1);
        
        left.SetValues(CAMERA_WIDTH, -1);
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

struct Pair
{
    public int x;
    public int y;
}